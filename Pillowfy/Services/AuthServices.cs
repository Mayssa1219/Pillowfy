using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pillowfy.DTOs.Auth;
using Pillowfy.Models;
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
	}

	public class AuthService : IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly IConfiguration _configuration;

		public AuthService(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IConfiguration configuration)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_configuration = configuration;
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

				// Assigner le rôle "Customer" par défaut
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
			try
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user == null)
					return new AuthResponseDto
					{
						Success = false,
						Message = "Email ou mot de passe incorrect."
					};

				var passwordCorrect = await _userManager.CheckPasswordAsync(user, model.Password);
				if (!passwordCorrect)
					return new AuthResponseDto
					{
						Success = false,
						Message = "Email ou mot de passe incorrect."
					};

				var roles = await _userManager.GetRolesAsync(user);
				var token = GenerateJwtToken(user, roles);

				return new AuthResponseDto
				{
					Success = true,
					Message = "Connexion réussie.",
					Token = token,
					User = new UserDto
					{
						Id = user.Id,
						Email = user.Email,
						FirstName = user.FirstName,
						LastName = user.LastName,
						PhoneNumber = user.PhoneNumber,
						Roles = roles.ToList()
					}
				};
			}
			catch (Exception ex)
			{
				return new AuthResponseDto
				{
					Success = false,
					Message = $"Erreur lors de la connexion: {ex.Message}"
				};
			}
		}

		public async Task<AuthResponseDto> AssignRoleAsync(string userId, string role)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(userId);
				if (user == null)
					return new AuthResponseDto
					{
						Success = false,
						Message = "Utilisateur non trouvé."
					};

				var roleExists = await _roleManager.RoleExistsAsync(role);
				if (!roleExists)
					return new AuthResponseDto
					{
						Success = false,
						Message = $"Le rôle '{role}' n'existe pas."
					};

				var result = await _userManager.AddToRoleAsync(user, role);
				if (!result.Succeeded)
					return new AuthResponseDto
					{
						Success = false,
						Message = string.Join(", ", result.Errors.Select(e => e.Description))
					};

				return new AuthResponseDto
				{
					Success = true,
					Message = $"Rôle '{role}' assigné avec succès."
				};
			}
			catch (Exception ex)
			{
				return new AuthResponseDto
				{
					Success = false,
					Message = $"Erreur lors de l'assignation du rôle: {ex.Message}"
				};
			}
		}

		private string GenerateJwtToken(ApplicationUser user, IList<string> roles)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
			};

			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddHours(24),
				Issuer = _configuration["Jwt:Issuer"],
				Audience = _configuration["Jwt:Audience"],
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}
	}
}