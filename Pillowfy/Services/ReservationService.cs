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
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.Id == dto.ChambreId && c.IsActive)
                ?? throw new Exception("Chambre introuvable.");

            var isBooked = await _context.Reservations.AnyAsync(r =>
                r.ChambreId == dto.ChambreId &&
                r.Status != ReservationStatus.Cancelled &&
                r.CheckIn < dto.CheckOut &&
                r.CheckOut > dto.CheckIn);

            if (isBooked)
                throw new Exception("Chambre non disponible pour ces dates.");

            var nights = Math.Max(1, (dto.CheckOut - dto.CheckIn).Days);
            var prixBase = nights * chambre.PricePerNight;

            // ── Gestion des points ──────────────────────────────────
            int pointsUtilises = 0;
            decimal remise = 0;

            if (dto.PointsUtilises > 0)
            {
                // Calcul des points disponibles = total dépensé non annulé
                var totalDepense = await _context.Reservations
                    .Where(r => r.UserId == userId && r.Status != ReservationStatus.Cancelled)
                    .SumAsync(r => r.TotalPrice);

                var pointsDispo = (int)totalDepense;

                if (dto.PointsUtilises > pointsDispo)
                    throw new Exception($"Points insuffisants. Vous avez {pointsDispo} pts disponibles.");

                // Points doivent être multiples de 100
                var ptsValides = (dto.PointsUtilises / 100) * 100;
                if (ptsValides <= 0)
                    throw new Exception("Le minimum est 100 points (= 10 €).");

                // 100 pts = 10 €
                remise = (ptsValides / 100m) * 10m;

                // Plafond : max 50% du prix de base
                var plafond = Math.Floor(prixBase * 0.5m);
                if (remise > plafond)
                {
                    remise = plafond;
                    ptsValides = (int)(Math.Floor(remise / 10m)) * 100;
                }

                pointsUtilises = ptsValides;
            }
            // ────────────────────────────────────────────────────────

            var totalFinal = Math.Max(0, prixBase - remise);

            var reservation = new Reservation
            {
                ChambreId = dto.ChambreId,
                UserId = userId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                NumberOfGuests = dto.NumberOfGuests,
                TotalPrice = totalFinal,
                Status = ReservationStatus.Confirmed
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = reservation.Id,
                ChambreId = chambre.Id,
                ChambreName = chambre.Name,
                HotelName = chambre.Hotel?.Name ?? "",
                HotelImageUrl = chambre.Hotel?.ImageUrl,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut,
                NumberOfGuests = reservation.NumberOfGuests,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString(),
                PrixBase = prixBase,
                RemisePoints = remise,
                PointsUtilises = pointsUtilises
            };
        }

        public async Task<List<ReservationDto>> GetUserReservationsAsync(string userId)
        {
            return await _context.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .Where(r => r.UserId == userId)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    ChambreId = r.ChambreId,
                    ChambreName = r.Chambre.Name,
                    HotelName = r.Chambre.Hotel.Name,
                    HotelImageUrl = r.Chambre.Hotel.ImageUrl,
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

        public async Task<byte[]?> GenerateInvoiceAsync(int reservationId, string userId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId);

            if (reservation == null) return null;

            var nights = Math.Max(1, (reservation.CheckOut - reservation.CheckIn).Days);

            var content = $"""
                PILLOWFY — FACTURE
                ══════════════════════════════════
                N° : FAC-{reservation.CheckOut.Year}-{reservation.Id:D4}
                Date : {DateTime.UtcNow:dd/MM/yyyy}

                SÉJOUR
                ──────────────────────────────────
                Hôtel    : {reservation.Chambre.Hotel.Name}
                Chambre  : {reservation.Chambre.Name}
                Arrivée  : {reservation.CheckIn:dd/MM/yyyy}
                Départ   : {reservation.CheckOut:dd/MM/yyyy}
                Durée    : {nights} nuit(s)
                Personnes: {reservation.NumberOfGuests}

                TARIF
                ──────────────────────────────────
                Prix/nuit : {reservation.Chambre.PricePerNight:N0} €
                Total TTC : {reservation.TotalPrice:N0} €

                Merci de votre confiance — Pillowfy
                ══════════════════════════════════
                """;

            return System.Text.Encoding.UTF8.GetBytes(content);
        }
        // À AJOUTER dans Services/ReservationService.cs

        public async Task<ReservationDto> UpdateAsync(int reservationId, string userId, ReservationUpdateDto dto)
        {
            if (dto.CheckIn >= dto.CheckOut)
                throw new Exception("La date de départ doit être après la date d'arrivée.");

            if (dto.CheckIn.Date <= DateTime.UtcNow.Date)
                throw new Exception("Impossible de modifier une réservation dont le check-in est déjà passé.");

            var reservation = await _context.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.UserId == userId)
                ?? throw new Exception("Réservation introuvable.");

            if (reservation.Status == ReservationStatus.Cancelled)
                throw new Exception("Impossible de modifier une réservation annulée.");

            // Vérifier disponibilité (en excluant la réservation actuelle)
            var isBooked = await _context.Reservations.AnyAsync(r =>
                r.Id != reservationId &&
                r.ChambreId == reservation.ChambreId &&
                r.Status != ReservationStatus.Cancelled &&
                r.CheckIn < dto.CheckOut &&
                r.CheckOut > dto.CheckIn);

            if (isBooked)
                throw new Exception("La chambre n'est pas disponible pour ces nouvelles dates.");

            var nights = Math.Max(1, (dto.CheckOut - dto.CheckIn).Days);

            reservation.CheckIn = dto.CheckIn;
            reservation.CheckOut = dto.CheckOut;
            reservation.NumberOfGuests = dto.NumberOfGuests;
            reservation.TotalPrice = nights * reservation.Chambre.PricePerNight;

            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = reservation.Id,
                ChambreId = reservation.ChambreId,
                ChambreName = reservation.Chambre.Name,
                HotelName = reservation.Chambre.Hotel?.Name ?? "",
                HotelImageUrl = reservation.Chambre.Hotel?.ImageUrl,
                CheckIn = reservation.CheckIn,
                CheckOut = reservation.CheckOut,
                NumberOfGuests = reservation.NumberOfGuests,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status.ToString()
            };
        }

        public async Task<AvailabilityResultDto> CheckAvailabilityAsync(int chambreId, DateTime checkIn, DateTime checkOut, int excludeReservationId = 0)
        {
            var isBooked = await _context.Reservations.AnyAsync(r =>
                r.ChambreId == chambreId &&
                r.Id != excludeReservationId &&
                r.Status != ReservationStatus.Cancelled &&
                r.CheckIn < checkOut &&
                r.CheckOut > checkIn);

            return new AvailabilityResultDto
            {
                Available = !isBooked,
                Reason = isBooked ? "Chambre déjà réservée pour ces dates." : null
            };
        }

    }
}