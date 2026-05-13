using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class StatistiqueService
    {
        private readonly PilloWfyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatistiqueService(PilloWfyDbContext context,
                          UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;

        }

        public async Task<DashboardAdminDto> GetDashboardAsync()
        {

            // Dans le service :
            var totalOwners = (await _userManager.GetUsersInRoleAsync("Owner")).Count;
            var totalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count;

            // Nouveaux inscrits ce mois
            var debutMois = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var newUsersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= debutMois);
            // ── Réservations chargées en mémoire (avec Hotel inclus) ──
            var reservations = await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .ToListAsync();

            var confirmees = reservations
                .Where(r => r.Status == ReservationStatus.Confirmed)
                .ToList();

            // ✅ CORRIGÉ : (double?) + ?? 0 — EF Core traduit parfaitement en SQL
            var noteMoyenne = Math.Round(
                await _context.Avis.AverageAsync(a => (double?)a.Note) ?? 0.0
            , 1);

            int annee = DateTime.UtcNow.Year;

            // ── Réservations par mois — déjà en mémoire, pas de problème ──
            var parMois = Enumerable.Range(1, 12).Select(mois =>
            {
                var r = reservations
                    .Where(x => x.CheckIn.Year == annee
                             && x.CheckIn.Month == mois)
                    .ToList();

                return new ReservationsParMoisDto
                {
                    Mois = new DateTime(annee, mois, 1).ToString("MMM yyyy"),
                    NombreReservations = r.Count,
                    Revenu = r.Where(x => x.Status == ReservationStatus.Confirmed)
                                          .Sum(x => x.TotalPrice)
                };
            }).ToList();

            // ── Notes moyennes par hôtel — chargées en mémoire AVANT le GroupBy ──
            // ✅ CORRIGÉ : on charge tous les avis en mémoire une seule fois
            //             pour éviter le mélange EF Core / LINQ in-memory
            var avisParHotel = await _context.Avis
                .GroupBy(a => a.HotelId)
                .Select(g => new
                {
                    HotelId = g.Key,
                    NoteMoyenne = g.Average(a => (double)a.Note)
                })
                .ToListAsync();  // ← tout en SQL, résultat en mémoire

            // ── Hôtels populaires — tout en mémoire maintenant ──
            var hotelsPopulaires = reservations
                .Where(r => r.Chambre?.Hotel != null)
                .GroupBy(r => new
                {
                    r.Chambre.Hotel.Id,
                    r.Chambre.Hotel.Name,
                    r.Chambre.Hotel.City
                })
                .Select(g =>
                {
                    // Lookup dans la liste déjà chargée — pas de requête SQL ici
                    var note = avisParHotel
                        .FirstOrDefault(a => a.HotelId == g.Key.Id)
                        ?.NoteMoyenne ?? 0.0;

                    return new HotelPopulaireDto
                    {
                        HotelName = g.Key.Name,
                        City = g.Key.City,
                        NombreReservations = g.Count(),
                        NoteMoyenne = Math.Round(note, 1)
                    };
                })
                .OrderByDescending(h => h.NombreReservations)
                .Take(5)
                .ToList();

            return new DashboardAdminDto
            {
                TotalReservations = reservations.Count,
                ReservationsConfirmees = confirmees.Count,
                ReservationsAnnulees = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                RevenuTotal = confirmees.Sum(r => r.TotalPrice),
                TauxOccupation = await GetTauxOccupationAsync(),
                TotalHotels = await _context.Hotels.CountAsync(),
                TotalChambres = await _context.Chambres.CountAsync(),
                TotalClients = totalOwners + totalCustomers,
                TotalAvis = await _context.Avis.CountAsync(),
                NoteMoyenneGlobale = noteMoyenne,

                TotalOwners = totalOwners,
                TotalCustomers = totalCustomers,
                NewUsersThisMonth = newUsersThisMonth,

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
                .Distinct()
                .CountAsync();

            return Math.Round((double)occupees / total * 100, 1);
        }
    }
}