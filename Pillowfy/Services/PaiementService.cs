using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class PaiementService
    {
        private readonly PilloWfyDbContext _context;

        public PaiementService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<PaiementResponseDto> ProcessPaymentAsync(
            PaiementCreateDto dto, string userId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

            if (reservation == null)
                throw new Exception("Réservation introuvable.");

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException(
                    "Cette réservation ne vous appartient pas.");

            var dejaPayee = await _context.Paiements
                .AnyAsync(p => p.ReservationId == dto.ReservationId
                            && p.Statut == StatutPaiement.Confirme);

            if (dejaPayee)
                throw new Exception("Cette réservation est déjà payée.");

            var paiement = new Paiement
            {
                ReservationId = dto.ReservationId,
                Montant = reservation.TotalPrice,
                DatePaiement = DateTime.UtcNow,
                Methode = dto.Methode,
                Statut = StatutPaiement.Confirme
            };

            _context.Paiements.Add(paiement);
            reservation.Status = ReservationStatus.Confirmed;
            await _context.SaveChangesAsync();

            return new PaiementResponseDto
            {
                Id = paiement.Id,
                Montant = paiement.Montant,
                DatePaiement = paiement.DatePaiement,
                Methode = paiement.Methode.ToString(),
                Statut = paiement.Statut.ToString(),
                ReservationId = paiement.ReservationId,
                HotelName = reservation.Chambre?.Hotel?.Name,
                ChambreName = reservation.Chambre?.Name
            };
        }

        public async Task<PaiementResponseDto?> GetPaymentByReservationAsync(
            int reservationId)
        {
            var paiement = await _context.Paiements
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Chambre)
                        .ThenInclude(c => c.Hotel)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (paiement == null) return null;

            return new PaiementResponseDto
            {
                Id = paiement.Id,
                Montant = paiement.Montant,
                DatePaiement = paiement.DatePaiement,
                Methode = paiement.Methode.ToString(),
                Statut = paiement.Statut.ToString(),
                ReservationId = paiement.ReservationId,
                HotelName = paiement.Reservation?.Chambre?.Hotel?.Name,
                ChambreName = paiement.Reservation?.Chambre?.Name
            };
        }

        public async Task<List<PaiementResponseDto>> GetAllPaymentsAsync()
        {
            return await _context.Paiements
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Chambre)
                        .ThenInclude(c => c.Hotel)
                .OrderByDescending(p => p.DatePaiement)
                .Select(p => new PaiementResponseDto
                {
                    Id = p.Id,
                    Montant = p.Montant,
                    DatePaiement = p.DatePaiement,
                    Methode = p.Methode.ToString(),
                    Statut = p.Statut.ToString(),
                    ReservationId = p.ReservationId,
                    HotelName = p.Reservation.Chambre.Hotel.Name,
                    ChambreName = p.Reservation.Chambre.Name
                })
                .ToListAsync();
        }

        public async Task<bool> RefundPaymentAsync(int paiementId)
        {
            var paiement = await _context.Paiements
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.Id == paiementId);

            if (paiement == null) return false;

            paiement.Statut = StatutPaiement.Rembourse;
            paiement.Reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}