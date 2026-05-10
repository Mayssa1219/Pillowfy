using Pillowfy.DTOs;

namespace Pillowfy.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateAsync(string userId, ReservationCreateDto dto);
        Task<List<ReservationDto>> GetUserReservationsAsync(string userId);
        Task<bool> CancelAsync(int reservationId, string userId);
        Task<byte[]?> GenerateInvoiceAsync(int reservationId, string userId); // ← ajouté
        Task<ReservationDto> UpdateAsync(int id, string userId, ReservationUpdateDto dto);

    }
}