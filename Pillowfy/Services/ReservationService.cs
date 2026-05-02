using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Interfaces;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class ReservationService : IReservationService
    {
        private readonly PilloWfyDbContext _context;

        public ReservationService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<ReservationDto> CreateAsync(string userId, ReservationCreateDto dto)
        {
            if (dto.CheckIn >= dto.CheckOut)
                throw new Exception("Dates invalides.");

            var chambre = await _context.Chambres
                .FirstOrDefaultAsync(c => c.Id == dto.ChambreId && c.IsActive);

            if (chambre == null)
                throw new Exception("Chambre introuvable.");

            // 🔥 Vérifier disponibilité
            var isBooked = await _context.Reservations.AnyAsync(r =>
                r.ChambreId == dto.ChambreId &&
                r.Status != ReservationStatus.Cancelled &&
                r.CheckIn < dto.CheckOut &&
                r.CheckOut > dto.CheckIn
            );

            if (isBooked)
                throw new Exception("Chambre non disponible pour ces dates.");

            var nights = (dto.CheckOut - dto.CheckIn).Days;
            if (nights <= 0) nights = 1;

            var reservation = new Reservation
            {
                ChambreId = dto.ChambreId,
                UserId = userId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                NumberOfGuests = dto.NumberOfGuests,
                TotalPrice = nights * chambre.PricePerNight,
                Status = ReservationStatus.Confirmed
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = reservation.Id,
                ChambreId = chambre.Id,
                ChambreName = chambre.Name,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut,
                NumberOfGuests = reservation.NumberOfGuests,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString()
            };
        }

        public async Task<List<ReservationDto>> GetUserReservationsAsync(string userId)
        {
            return await _context.Reservations
                .Include(r => r.Chambre)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    ChambreId = r.ChambreId,
                    ChambreName = r.Chambre.Name,
                    CheckIn = r.CheckIn,
                    CheckOut = r.CheckOut,
                    NumberOfGuests = r.NumberOfGuests,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<bool> CancelAsync(int reservationId, string userId)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null)
                throw new Exception("Réservation introuvable.");

            reservation.Status = ReservationStatus.Cancelled;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}