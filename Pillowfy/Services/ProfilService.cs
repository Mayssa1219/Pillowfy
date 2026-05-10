// Services/ProfileService.cs
using Microsoft.AspNetCore.Identity;
using Pillowfy.DTOs;
using Pillowfy.Interfaces;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ProfileDto?> GetAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        public async Task<ProfileDto> UpdateAsync(string userId, ProfileUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Utilisateur introuvable.");

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Utilisateur introuvable.");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var isWrongPwd = result.Errors.Any(e =>
                    e.Code == "PasswordMismatch" || e.Description.Contains("incorrect", StringComparison.OrdinalIgnoreCase));
                throw new Exception(isWrongPwd ? "Mot de passe actuel incorrect." : string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task DeactivateAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception("Utilisateur introuvable.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
}