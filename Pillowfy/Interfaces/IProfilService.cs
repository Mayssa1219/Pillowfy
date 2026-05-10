using Pillowfy.DTOs;

namespace Pillowfy.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileDto?> GetAsync(string userId);
        Task<ProfileDto> UpdateAsync(string userId, ProfileUpdateDto dto);
        Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task DeactivateAsync(string userId);
    }
}