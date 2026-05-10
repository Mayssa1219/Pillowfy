using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Interfaces;

namespace Pillowfy.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly PilloWfyDbContext _context;

        public DashboardService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetClientDashboardAsync(string userId)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var completed = reservations
                .Where(r => r.Status == ReservationStatus.Confirmed
                         && r.CheckOut <= DateTime.UtcNow)
                .ToList();

            var nuitsTotal = completed
                .Sum(r => (r.CheckOut - r.CheckIn).Days);

            var totalDepense = reservations
                .Where(r => r.Status != ReservationStatus.Cancelled)
                .Sum(r => r.TotalPrice);

            var points = (int)totalDepense;
            var (tier, nextTierName, nextTierPts, progress) = ComputeLoyalty(points);

            var today = DateTime.UtcNow.Date;

            var active = reservations
                .Where(r => r.Status == ReservationStatus.Confirmed
                         && r.CheckOut.Date > today)
                .OrderBy(r => r.CheckIn)
                .FirstOrDefault();

            ReservationDto? activeDto = null;
            if (active != null)
            {
                activeDto = new ReservationDto
                {
                    Id = active.Id,
                    ChambreId = active.ChambreId,
                    ChambreName = active.Chambre?.Name ?? "",
                    HotelName = active.Chambre?.Hotel?.Name ?? "",
                    HotelImageUrl = active.Chambre?.Hotel?.ImageUrl,
                    CheckIn = active.CheckIn,
                    CheckOut = active.CheckOut,
                    NumberOfGuests = active.NumberOfGuests,
                    TotalPrice = active.TotalPrice,
                    Status = active.Status.ToString()
                };
            }

            // ── Dernières réservations (5 max) ──
            var dernieresReservations = reservations
                .OrderByDescending(r => r.CheckIn)
                .Take(5)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    ChambreId = r.ChambreId,
                    ChambreName = r.Chambre?.Name ?? "",
                    HotelName = r.Chambre?.Hotel?.Name ?? "",
                    HotelImageUrl = r.Chambre?.Hotel?.ImageUrl,
                    CheckIn = r.CheckIn,
                    CheckOut = r.CheckOut,
                    NumberOfGuests = r.NumberOfGuests,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString()
                })
                .ToList();

            // ── Dernier paiement ──
            var dernierPaiement = await _context.Paiements
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Chambre)
                        .ThenInclude(c => c.Hotel)
                .Where(p => p.Reservation.UserId == userId)
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
                .FirstOrDefaultAsync();

            // ── Dernier avis ──
            var dernierAvis = await _context.Avis
                .Include(a => a.Hotel)
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.DateAvis)
                .Select(a => new AvisResponseDto
                {
                    Id = a.Id,
                    Note = a.Note,
                    Commentaire = a.Commentaire,
                    DateAvis = a.DateAvis,
                    HotelId = a.HotelId,
                    HotelName = a.Hotel.Name,
                    AuteurNom = a.User.FirstName + " " + a.User.LastName
                })
                .FirstOrDefaultAsync();

            return new DashboardDto
            {
                ReservationsTotal = reservations.Count(r => r.Status != ReservationStatus.Cancelled),
                ReservationsActives = reservations.Count(r => r.Status == ReservationStatus.Confirmed
                                                             && r.CheckOut.Date > today),
                ReservationsAnnulees = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                NuitsTotal = nuitsTotal,
                PointsFidelite = points,
                Tier = tier,
                TotalDepense = totalDepense.ToString("N0", new System.Globalization.CultureInfo("fr-FR")) + " €",
                NextTierPts = nextTierPts,
                NextTierName = nextTierName,
                LoyaltyProgress = progress,
                ActiveReservation = activeDto,
                DernieresReservations = dernieresReservations,
                DernierPaiement = dernierPaiement,
                DernierAvis = dernierAvis
            };
        }

        public async Task<List<OccupancyDto>> GetOccupancyAsync()
        {
            var today = DateTime.UtcNow.Date;

            var hotels = await _context.Hotels
                .Where(h => h.IsActive)
                .Include(h => h.Chambres)
                    .ThenInclude(c => c.Reservations)
                .ToListAsync();

            return hotels.Select(h =>
            {
                var total = h.Chambres.Count(c => c.IsActive);
                var occupied = total == 0 ? 0 : h.Chambres.Count(c =>
                    c.IsActive &&
                    c.Reservations.Any(r =>
                        r.Status != ReservationStatus.Cancelled &&
                        r.CheckIn.Date <= today &&
                        r.CheckOut.Date > today));

                var rate = total == 0 ? 0 : (int)Math.Round(100.0 * occupied / total);

                return new OccupancyDto
                {
                    HotelName = h.Name,
                    ImageUrl = h.ImageUrl,
                    OccupancyRate = rate
                };
            })
            .OrderByDescending(o => o.OccupancyRate)
            .ToList();
        }

        // ── Calcul tier fidélité ──
        private static (string tier, string nextTierName, int nextTierPts, int progress) ComputeLoyalty(int points)
        {
            // Paliers : Bronze 0, Silver 500, Gold 1000, Platinum 2000
            if (points >= 2000)
                return ("Platinum", "Platinum", 0, 100);

            if (points >= 1000)
            {
                var prog = (int)((points - 1000) / 10.0); // 0→100 entre 1000 et 2000
                return ("Gold", "Platinum", 2000 - points, Math.Min(prog, 99));
            }

            if (points >= 500)
            {
                var prog = (int)((points - 500) / 5.0);
                return ("Silver", "Gold", 1000 - points, Math.Min(prog, 99));
            }

            var p = (int)(points / 5.0);
            return ("Bronze", "Silver", 500 - points, Math.Min(p, 99));
        }
    }
}