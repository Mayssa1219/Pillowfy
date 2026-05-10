using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;

namespace Pillowfy.Services
{
    public class StatistiqueService
    {
        private readonly PilloWfyDbContext _context;

        public StatistiqueService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAdminDto> GetDashboardAsync()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .ToListAsync();

            var confirmees = reservations
                .Where(r => r.Status == ReservationStatus.Confirmed).ToList();

            var noteMoyenne = await _context.Avis.AnyAsync()
                ? Math.Round(await _context.Avis
                    .AverageAsync(a => (double)a.Note), 1)
                : 0;

            int annee = DateTime.UtcNow.Year;

            var parMois = Enumerable.Range(1, 12).Select(mois =>
            {
                var r = reservations
                    .Where(x => x.CheckIn.Year == annee
                             && x.CheckIn.Month == mois).ToList();
                return new ReservationsParMoisDto
                {
                    Mois = new DateTime(annee, mois, 1).ToString("MMM yyyy"),
                    NombreReservations = r.Count,
                    Revenu = r.Where(x => x.Status == ReservationStatus.Confirmed)
                               .Sum(x => x.TotalPrice)
                };
            }).ToList();

            var hotelsPopulaires = reservations
                .Where(r => r.Chambre?.Hotel != null)
                .GroupBy(r => new
                {
                    r.Chambre.Hotel.Id,
                    r.Chambre.Hotel.Name,
                    r.Chambre.Hotel.City
                })
                .Select(g => new HotelPopulaireDto
                {
                    HotelName = g.Key.Name,
                    City = g.Key.City,
                    NombreReservations = g.Count(),
                    NoteMoyenne = _context.Avis
                        .Where(a => a.HotelId == g.Key.Id)
                        .Select(a => (double)a.Note)
                        .DefaultIfEmpty(0).Average()
                })
                .OrderByDescending(h => h.NombreReservations)
                .Take(5).ToList();

            return new DashboardAdminDto
            {
                TotalReservations = reservations.Count,
                ReservationsConfirmees = confirmees.Count,
                ReservationsAnnulees = reservations
                    .Count(r => r.Status == ReservationStatus.Cancelled),
                RevenuTotal = confirmees.Sum(r => r.TotalPrice),
                TotalHotels = await _context.Hotels.CountAsync(),
                TotalChambres = await _context.Chambres.CountAsync(),
                TotalClients = await _context.Users.CountAsync(),
                TotalAvis = await _context.Avis.CountAsync(),
                NoteMoyenneGlobale = noteMoyenne,
                ReservationsParMois = parMois,
                HotelsLesPlusReserves = hotelsPopulaires
            };
        }

        public async Task<decimal> GetRevenuTotalAsync()
        {
            return await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Confirmed)
                .SumAsync(r => r.TotalPrice);
        }

        public async Task<double> GetTauxOccupationAsync()
        {
            var total = await _context.Chambres.CountAsync();
            if (total == 0) return 0;
            var occupees = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Confirmed
                         && r.CheckIn <= DateTime.UtcNow
                         && r.CheckOut >= DateTime.UtcNow)
                .Select(r => r.ChambreId)
                .Distinct().CountAsync();
            return Math.Round((double)occupees / total * 100, 1);
        }
    }
}