using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class AvisService
    {
        private readonly PilloWfyDbContext _context;

        public AvisService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<AvisResponseDto> AddAvisAsync(
            AvisCreateDto dto, string userId)
        {
            var hotel = await _context.Hotels.FindAsync(dto.HotelId);
            if (hotel == null)
                throw new Exception("Hôtel introuvable.");

            var dejaLaisse = await _context.Avis
                .AnyAsync(a => a.UserId == userId && a.HotelId == dto.HotelId);
            if (dejaLaisse)
                throw new Exception(
                    "Vous avez déjà laissé un avis pour cet hôtel.");

            var avis = new Avis
            {
                Note = dto.Note,
                Commentaire = dto.Commentaire,
                HotelId = dto.HotelId,
                UserId = userId,
                DateAvis = DateTime.UtcNow
            };

            _context.Avis.Add(avis);
            await _context.SaveChangesAsync();
            await _context.Entry(avis).Reference(a => a.User).LoadAsync();

            return new AvisResponseDto
            {
                Id = avis.Id,
                Note = avis.Note,
                Commentaire = avis.Commentaire,
                DateAvis = avis.DateAvis,
                AuteurNom = avis.User != null
                    ? $"{avis.User.FirstName} {avis.User.LastName}"
                    : "Anonyme",
                HotelId = avis.HotelId,
                HotelName = hotel.Name
            };
        }

        public async Task<List<AvisResponseDto>> GetAvisByHotelAsync(int hotelId)
        {
            return await _context.Avis
                .Include(a => a.User)
                .Include(a => a.Hotel)
                .Where(a => a.HotelId == hotelId)
                .OrderByDescending(a => a.DateAvis)
                .Select(a => new AvisResponseDto
                {
                    Id = a.Id,
                    Note = a.Note,
                    Commentaire = a.Commentaire,
                    DateAvis = a.DateAvis,
                    AuteurNom = a.User.FirstName + " " + a.User.LastName,
                    HotelId = a.HotelId,
                    HotelName = a.Hotel.Name
                })
                .ToListAsync();
        }

        public async Task<List<AvisResponseDto>> GetAvisByUserAsync(string userId)
        {
            return await _context.Avis
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
                .ToListAsync();
        }

        public async Task<bool> DeleteAvisAsync(
            int avisId, string userId, bool isAdmin)
        {
            var avis = await _context.Avis.FindAsync(avisId);
            if (avis == null) return false;

            if (!isAdmin && avis.UserId != userId)
                throw new UnauthorizedAccessException(
                    "Vous ne pouvez pas supprimer cet avis.");

            _context.Avis.Remove(avis);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<AvisResponseDto> UpdateAvisAsync(int avisId, string userId, AvisUpdateDto dto)
        {
            var avis = await _context.Avis
                .Include(a => a.Hotel)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == avisId);

            if (avis == null)
                throw new Exception("Avis introuvable.");

            if (avis.UserId != userId)
                throw new UnauthorizedAccessException("Vous ne pouvez pas modifier cet avis.");

            avis.Note = dto.Note;
            avis.Commentaire = dto.Commentaire;

            await _context.SaveChangesAsync();

            return new AvisResponseDto
            {
                Id = avis.Id,
                Note = avis.Note,
                Commentaire = avis.Commentaire,
                DateAvis = avis.DateAvis,
                HotelId = avis.HotelId,
                HotelName = avis.Hotel?.Name,
                AuteurNom = avis.User != null ? $"{avis.User.FirstName} {avis.User.LastName}" : "Anonyme"
            };
        }
    }
}