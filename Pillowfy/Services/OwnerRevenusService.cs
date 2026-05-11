using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using System.Globalization;

namespace Pillowfy.Services
{
    public class OwnerRevenusService
    {
        private readonly PilloWfyDbContext _context;

        public OwnerRevenusService(PilloWfyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retourne :
        ///  - Les revenus mensuels sur les 12 derniers mois (glissants)
        ///  - La répartition des revenus cumulés par hôtel
        /// </summary>
        public async Task<RevenusResponseDto> GetRevenusAsync(string ownerId)
        {
            var now = DateTime.UtcNow;
            var culture = new CultureInfo("fr-FR");

            // Hôtels actifs du propriétaire
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            // Toutes les réservations non annulées pour ces hôtels
            // (on charge en mémoire pour éviter N+1 sur les 12 mois)
            var reservations = await _context.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId)
                         && r.Status != ReservationStatus.Cancelled)
                .Select(r => new
                {
                    r.TotalPrice,
                    r.CreatedAt,
                    HotelName = r.Chambre.Hotel.Name,
                    HotelId = r.Chambre.HotelId
                })
                .ToListAsync();

            // ── REVENUS MENSUELS (12 mois glissants) ──────────────
            var revenusMensuels = new List<RevenuMensuelDetailDto>();

            for (int i = 11; i >= 0; i--)
            {
                var moisRef = now.AddMonths(-i);
                var debut = new DateTime(moisRef.Year, moisRef.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var fin = debut.AddMonths(1);

                var montant = reservations
                    .Where(r => r.CreatedAt >= debut && r.CreatedAt < fin)
                    .Sum(r => r.TotalPrice);

                revenusMensuels.Add(new RevenuMensuelDetailDto
                {
                    Mois = moisRef.ToString("MMM", culture),   // "jan.", "fév." …
                    MoisIndex = moisRef.Month,
                    Annee = moisRef.Year,
                    Montant = montant
                });
            }

            // ── RÉPARTITION PAR HÔTEL ─────────────────────────────
            var parHotel = reservations
                .GroupBy(r => r.HotelName)
                .Select(g => new RevenuParHotelDto
                {
                    HotelName = g.Key,
                    Revenus = g.Sum(r => r.TotalPrice)
                })
                .OrderByDescending(h => h.Revenus)
                .ToList();

            return new RevenusResponseDto
            {
                RevenusMensuels = revenusMensuels,
                ParHotel = parHotel
            };
        }
    }
}