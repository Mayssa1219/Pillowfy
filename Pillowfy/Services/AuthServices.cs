using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pillowfy.DTOs.Auth;
using Pillowfy.Models;
using Pillowfy.Services.EmailService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pillowfy.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto model);
        Task<AuthResponseDto> LoginAsync(LoginDto model);
        Task<AuthResponseDto> AssignRoleAsync(string userId, string role);
        Task<AuthResponseDto> ForgotPasswordAsync(string email); // ← nouveau
        Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model);
    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService; 

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmailService emailService) // ← nouveau
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService; // ← nouveau
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto model)
        {
            try
            {
                var userExists = await _userManager.FindByEmailAsync(model.Email);
                if (userExists != null)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Un utilisateur avec cet email existe déjà."
                    };

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    };

                await _userManager.AddToRoleAsync(user, "Customer");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Inscription réussie. Veuillez vous connecter.",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PhoneNumber = user.PhoneNumber,
                        Roles = new List<string> { "Customer" }
                    }
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Erreur lors de l'inscription: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Fail("Email ou mot de passe incorrect.");

            // ✅ Vérifier si le compte est actif
            if (!user.IsActive)
                return Fail("Votre compte a été désactivé. Veuillez contacter l'administrateur.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!passwordValid)
                return Fail("Email ou mot de passe incorrect.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                Success = true,
                Token = token,
                Role = roles.FirstOrDefault(),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }

        private AuthResponseDto Fail(string msg)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = msg
            };
        }

        public async Task<AuthResponseDto> AssignRoleAsync(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Fail("Utilisateur non trouvé.");

                if (!await _roleManager.RoleExistsAsync(role))
                    return Fail($"Le rôle '{role}' n'existe pas.");

                // vérifier si déjà dans le rôle
                if (await _userManager.IsInRoleAsync(user, role))
                    return Fail("Utilisateur possède déjà ce rôle.");

                var result = await _userManager.AddToRoleAsync(user, role);

                if (!result.Succeeded)
                    return Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

                return new AuthResponseDto
                {
                    Success = true,
                    Message = $"Rôle '{role}' assigné avec succès."
                };
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "E-mail introuvable."
                    };

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var resetLink = $"https://localhost:7166/Login/ResetPassword?email={email}&token={encodedToken}";


                var body = $@"
                    <div style='font-family:sans-serif; max-width:500px; margin:auto;'>
                        <h2 style='color:#C9A96E;'>Pillowfy</h2>
                        <p>Bonjour <strong>{user.FirstName}</strong>,</p>
                        <p>Vous avez demandé à réinitialiser votre mot de passe.</p>
                        <a href='{resetLink}' style='
                            display:inline-block;
                            background:#C9A96E;
                            color:#0F1621;
                            padding:12px 28px;
                            border-radius:100px;
                            text-decoration:none;
                            font-weight:600;
                            margin:16px 0;
                        '>Réinitialiser mon mot de passe →</a>
                        <p style='color:#999; font-size:13px;'>Ce lien expire dans 24h. Si vous n'avez pas fait cette demande, ignorez cet e-mail.</p>
                    </div>
                ";

                await _emailService.SendEmailAsync(
                    email,
                    "Réinitialisation de mot de passe — Pillowfy",
                    body
                );

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Lien de réinitialisation envoyé par e-mail."
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                };
            }
        }
        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Utilisateur introuvable."
                    };

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (!result.Succeeded)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    };

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Mot de passe réinitialisé avec succès."
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Erreur: {ex.Message}"
                };
            }
        }
        private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName)
    };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }}