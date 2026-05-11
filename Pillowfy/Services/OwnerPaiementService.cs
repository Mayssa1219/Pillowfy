using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Models;
using System.Globalization;

namespace Pillowfy.Services
{
    public class OwnerPaiementService
    {
        private readonly PilloWfyDbContext _context;

        public OwnerPaiementService(PilloWfyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retourne tous les paiements liés aux hôtels du propriétaire,
        /// plus les statistiques agrégées pour le mois en cours.
        /// </summary>
        public async Task<PaiementsResponseDto> GetPaiementsAsync(string ownerId)
        {
            var now = DateTime.UtcNow;
            var debutMois = new DateTime(now.Year, now.Month, 1);

            // Récupération des IDs d'hôtels actifs appartenant au propriétaire
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            // Tous les paiements concernant ces hôtels, du plus récent au plus ancien
            var paiements = await _context.Paiements
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Chambre)
                        .ThenInclude(c => c.Hotel)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.User)
                .Where(p => hotelIds.Contains(p.Reservation.Chambre.HotelId))
                .OrderByDescending(p => p.DatePaiement)
                .ToListAsync();

            // ── STATS ──────────────────────────────────────────────
            var paiementsMois = paiements
                .Where(p => p.DatePaiement >= debutMois)
                .ToList();

            var stats = new PaiementStatsDto
            {
                Confirme = paiements.Count(p => p.Statut == StatutPaiement.Confirme),
                EnAttente = paiements.Count(p => p.Statut == StatutPaiement.EnAttente),
                Echoue = paiements.Count(p => p.Statut == StatutPaiement.Echoue),
                Rembourse = paiements.Count(p => p.Statut == StatutPaiement.Rembourse),
                TotalMois = paiementsMois
                    .Where(p => p.Statut == StatutPaiement.Confirme)
                    .Sum(p => p.Montant)
            };

            // ── LISTE ──────────────────────────────────────────────
            var dtos = paiements.Select(p => new PaiementOwnerDto
            {
                Id = p.Id,
                Montant = p.Montant,
                DatePaiement = p.DatePaiement.ToString("o"),          // ISO 8601
                Methode = p.Methode.ToString(),
                Statut = p.Statut.ToString(),
                ReservationId = p.ReservationId,
                ClientName = p.Reservation.User.FirstName + " " + p.Reservation.User.LastName,
                HotelName = p.Reservation.Chambre.Hotel.Name,
                ChambreName = p.Reservation.Chambre.Name
            }).ToList();

            return new PaiementsResponseDto
            {
                Stats = stats,
                Paiements = dtos
            };
        }
    }
}