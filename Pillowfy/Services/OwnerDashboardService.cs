using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class OwnerDashboardService
    {
        private readonly PilloWfyDbContext _context;

        public OwnerDashboardService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<OwnerDashboardDto> GetDashboardAsync(string ownerId)
        {
            var now = DateTime.UtcNow;
            var debutMois = new DateTime(now.Year, now.Month, 1);

            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            var hotelsTotal = hotelIds.Count;

            var reservationsMois = await _context.Reservations
                .Include(r => r.Chambre)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId)
                         && r.CreatedAt >= debutMois
                         && r.Status != ReservationStatus.Cancelled)
                .CountAsync();

            var revenusMois = await _context.Reservations
                .Include(r => r.Chambre)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId)
                         && r.CreatedAt >= debutMois
                         && r.Status != ReservationStatus.Cancelled)
                .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

            var totalChambres = await _context.Chambres
                .Where(c => hotelIds.Contains(c.HotelId) && c.IsActive)
                .CountAsync();

            var chambresOccupees = await _context.Chambres
                .Where(c => hotelIds.Contains(c.HotelId) && c.IsActive)
                .CountAsync(c => c.Reservations.Any(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.CheckIn <= now && r.CheckOut >= now));

            var tauxMoyen = totalChambres > 0
                ? (int)Math.Round((double)chambresOccupees / totalChambres * 100)
                : 0;

            var enAttente = await _context.Reservations
                .Include(r => r.Chambre)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId)
                         && r.Status == ReservationStatus.Confirmed
                         && r.CheckIn > now)
                .CountAsync();

            var dernieres = await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .Include(r => r.User)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId))
                .OrderByDescending(r => r.CreatedAt)
                .Take(8)
                .Select(r => new OwnerReservationDto
                {
                    Id = r.Id,
                    ClientName = r.User.FirstName + " " + r.User.LastName,
                    ChambreName = r.Chambre.Name,
                    HotelName = r.Chambre.Hotel.Name,
                    CheckIn = r.CheckIn,
                    CheckOut = r.CheckOut,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            var chambres = await _context.Chambres
                .Include(c => c.Hotel)
                .Include(c => c.Reservations)
                .Where(c => hotelIds.Contains(c.HotelId) && c.IsActive)
                .Take(6)
                .Select(c => new OwnerChambreDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    HotelName = c.Hotel.Name,
                    ImageUrl = null,
                    Capacity = c.Capacity,
                    PricePerNight = c.PricePerNight,
                    IsAvailable = !c.Reservations.Any(r =>
                        r.Status != ReservationStatus.Cancelled &&
                        r.CheckIn <= now && r.CheckOut >= now)
                })
                .ToListAsync();

            var revenusMensuels = new List<RevenuMensuelDto>();
            for (int i = 5; i >= 0; i--)
            {
                var mois = now.AddMonths(-i);
                var debut = new DateTime(mois.Year, mois.Month, 1);
                var fin = debut.AddMonths(1);

                var montant = await _context.Reservations
                    .Include(r => r.Chambre)
                    .Where(r => hotelIds.Contains(r.Chambre.HotelId)
                             && r.Status != ReservationStatus.Cancelled
                             && r.CreatedAt >= debut && r.CreatedAt < fin)
                    .SumAsync(r => (decimal?)r.TotalPrice) ?? 0;

                revenusMensuels.Add(new RevenuMensuelDto
                {
                    Mois = mois.ToString("MMM", new System.Globalization.CultureInfo("fr-FR")),
                    Montant = montant
                });
            }

            return new OwnerDashboardDto
            {
                HotelsTotal = hotelsTotal,
                ReservationsMois = reservationsMois,
                RevenusMois = revenusMois,
                TauxOccupationMoyen = tauxMoyen,
                ReservationsEnAttente = enAttente,
                DernieresReservations = dernieres,
                MesChambresDispo = chambres,
                RevenusMensuels = revenusMensuels
            };
        }

        public async Task<List<OccupancyDto>> GetOccupancyAsync(string ownerId)
        {
            var now = DateTime.UtcNow;

            var hotels = await _context.Hotels
                .Include(h => h.Chambres)
                    .ThenInclude(c => c.Reservations)
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .ToListAsync();

            return hotels.Select(h =>
            {
                var total = h.Chambres.Count(c => c.IsActive);
                var occupees = h.Chambres.Count(c => c.IsActive && c.Reservations.Any(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.CheckIn <= now && r.CheckOut >= now));

                return new OccupancyDto
                {
                    HotelName = h.Name,
                    ImageUrl = h.ImageUrl,
                    OccupancyRate = total > 0 ? (int)Math.Round((double)occupees / total * 100) : 0
                };
            }).ToList();
        }

        public async Task<List<HotelOwnerDto>> GetHotelsAsync(string ownerId)
        {
            var now = DateTime.UtcNow;

            var hotels = await _context.Hotels
                .Include(h => h.Chambres)
                    .ThenInclude(c => c.Reservations)
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();

            return hotels.Select(h =>
            {
                var totalChambres = h.Chambres.Count(c => c.IsActive);
                var occupees = h.Chambres.Count(c => c.IsActive && c.Reservations.Any(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.CheckIn <= now && r.CheckOut >= now));
                var reservationsCount = h.Chambres
                    .SelectMany(c => c.Reservations)
                    .Count(r => r.Status != ReservationStatus.Cancelled);

                return new HotelOwnerDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Description = h.Description,
                    Address = h.Address,
                    City = h.City,
                    Country = h.Country,
                    ImageUrl = h.ImageUrl,
                    ChambresCount = totalChambres,
                    ReservationsCount = reservationsCount,
                    OccupancyRate = totalChambres > 0
                        ? (int)Math.Round((double)occupees / totalChambres * 100)
                        : 0
                };
            }).ToList();
        }

        public async Task<HotelOwnerDto> CreateHotelAsync(string ownerId, HotelCreateDto dto)
        {
            var hotel = new Hotel
            {
                Name = dto.Name,
                Description = dto.Description ?? "",
                Address = dto.Address ?? "",
                City = dto.City,
                Country = dto.Country,
                ImageUrl = dto.ImageUrl,   // chemin local ex: /uploads/hotels/abc.jpg
                OwnerId = ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            return new HotelOwnerDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Address = hotel.Address,
                City = hotel.City,
                Country = hotel.Country,
                ImageUrl = hotel.ImageUrl
            };
        }

        public async Task<HotelOwnerDto?> UpdateHotelAsync(string ownerId, int hotelId, HotelCreateDto dto)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.OwnerId == ownerId && h.IsActive);

            if (hotel == null) return null;

            hotel.Name = dto.Name;
            hotel.Description = dto.Description ?? "";
            hotel.Address = dto.Address ?? "";
            hotel.City = dto.City;
            hotel.Country = dto.Country;
            hotel.UpdatedAt = DateTime.UtcNow;

            // ✅ On ne remplace l'image que si une nouvelle a été uploadée
            if (!string.IsNullOrEmpty(dto.ImageUrl))
                hotel.ImageUrl = dto.ImageUrl;

            await _context.SaveChangesAsync();

            return new HotelOwnerDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Description = hotel.Description,
                Address = hotel.Address,
                City = hotel.City,
                Country = hotel.Country,
                ImageUrl = hotel.ImageUrl
            };
        }
        public async Task<List<OwnerReservatioDetailDto>> GetReservationsAsync(string ownerId)
        {
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            return await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .Include(r => r.User)
                .Where(r => hotelIds.Contains(r.Chambre.HotelId))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new OwnerReservatioDetailDto
                {
                    Id = r.Id,
                    ClientName = r.User.FirstName + " " + r.User.LastName,
                    ClientEmail = r.User.Email,
                    ChambreName = r.Chambre.Name,
                    HotelName = r.Chambre.Hotel.Name,
                    CheckIn = r.CheckIn,
                    CheckOut = r.CheckOut,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status.ToString(),
                    NumberOfGuests = r.NumberOfGuests
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateReservationStatusAsync(string ownerId, int reservationId, string newStatus)
        {
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            var reservation = await _context.Reservations
                .Include(r => r.Chambre)
                .FirstOrDefaultAsync(r => r.Id == reservationId
                                       && hotelIds.Contains(r.Chambre.HotelId));

            if (reservation == null) return false;

            if (!Enum.TryParse<ReservationStatus>(newStatus, out var status))
                return false;

            reservation.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHotelAsync(string ownerId, int hotelId)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == hotelId && h.OwnerId == ownerId);

            if (hotel == null) return false;

            hotel.IsActive = false;   // soft delete
            hotel.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<ChambreOwnerDto>> GetChambresAsync(string ownerId)
        {
            var now = DateTime.UtcNow;
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            return await _context.Chambres
                .Include(c => c.Hotel)
                .Include(c => c.Reservations)
                .Where(c => hotelIds.Contains(c.HotelId))
                .OrderBy(c => c.HotelId).ThenBy(c => c.Name)
                .Select(c => new ChambreOwnerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    PricePerNight = c.PricePerNight,
                    Capacity = c.Capacity,
                    IsActive = c.IsActive,
                    HotelId = c.HotelId,
                    HotelName = c.Hotel.Name,
                    ImageUrl = null,
                    IsAvailable = !c.Reservations.Any(r =>
                        r.Status != ReservationStatus.Cancelled &&
                        r.CheckIn <= now && r.CheckOut >= now),
                    ReservationsCount = c.Reservations
                        .Count(r => r.Status != ReservationStatus.Cancelled)
                })
                .ToListAsync();
        }

        public async Task<ChambreOwnerDto> CreateChambreAsync(string ownerId, ChambreCreateDto dto, string imageUrl)
        {
            var hotel = await _context.Hotels
                .FirstOrDefaultAsync(h => h.Id == dto.HotelId && h.OwnerId == ownerId && h.IsActive);
            if (hotel == null) return null;

            var chambre = new Chambre
            {
                Name = dto.Name,
                Description = dto.Description ?? "",
                PricePerNight = dto.PricePerNight,
                Capacity = dto.Capacity,
                HotelId = dto.HotelId,
                IsActive = true
            };

            _context.Chambres.Add(chambre);
            await _context.SaveChangesAsync();

            return new ChambreOwnerDto
            {
                Id = chambre.Id,
                Name = chambre.Name,
                Description = chambre.Description,
                PricePerNight = chambre.PricePerNight,
                Capacity = chambre.Capacity,
                IsActive = chambre.IsActive,
                HotelId = chambre.HotelId,
                HotelName = hotel.Name,
                ImageUrl = imageUrl
            };
        }

        public async Task<ChambreOwnerDto> UpdateChambreAsync(string ownerId, int chambreId, ChambreCreateDto dto, string imageUrl)
        {
            var chambre = await _context.Chambres
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.Id == chambreId && c.Hotel.OwnerId == ownerId);
            if (chambre == null) return null;

            chambre.Name = dto.Name;
            chambre.Description = dto.Description ?? "";
            chambre.PricePerNight = dto.PricePerNight;
            chambre.Capacity = dto.Capacity;

          

            await _context.SaveChangesAsync();

            return new ChambreOwnerDto
            {
                Id = chambre.Id,
                Name = chambre.Name,
                Description = chambre.Description,
                PricePerNight = chambre.PricePerNight,
                Capacity = chambre.Capacity,
                IsActive = chambre.IsActive,
                HotelId = chambre.HotelId,
                HotelName = chambre.Hotel.Name
            };
        }

        public async Task<bool> DeleteChambreAsync(string ownerId, int chambreId)
        {
            var chambre = await _context.Chambres
                .Include(c => c.Hotel)
                .FirstOrDefaultAsync(c => c.Id == chambreId && c.Hotel.OwnerId == ownerId);
            if (chambre == null) return false;

            chambre.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }


    }
}