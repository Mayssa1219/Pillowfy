using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;

namespace Pillowfy.Services
{
    public class OwnerAvisService
    {
        private readonly PilloWfyDbContext _context;

        public OwnerAvisService(PilloWfyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retourne tous les avis déposés sur les hôtels du propriétaire,
        /// du plus récent au plus ancien.
        /// </summary>
        public async Task<List<AvisOwnerDto>> GetAvisAsync(string ownerId)
        {
            // Hôtels actifs du propriétaire
            var hotelIds = await _context.Hotels
                .Where(h => h.OwnerId == ownerId && h.IsActive)
                .Select(h => h.Id)
                .ToListAsync();

            var avis = await _context.Avis
                .Include(a => a.User)
                .Include(a => a.Hotel)
                .Where(a => hotelIds.Contains(a.HotelId))
                .OrderByDescending(a => a.DateAvis)
                .Select(a => new AvisOwnerDto
                {
                    Id = a.Id,
                    Note = a.Note,
                    Commentaire = a.Commentaire,
                    DateAvis = a.DateAvis.ToString("o"),           // ISO 8601
                    ClientName = a.User.FirstName + " " + a.User.LastName,
                    HotelName = a.Hotel.Name
                })
                .ToListAsync();

            return avis;
        }
    }
}