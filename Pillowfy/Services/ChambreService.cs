using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.Enums;
using Pillowfy.Interfaces;
using Pillowfy.Models;

namespace Pillowfy.Services
{
    public class ChambreService : IChambreService
    {
        private readonly PilloWfyDbContext _context;

        public ChambreService(PilloWfyDbContext context)
        {
            _context = context;
        }

        public async Task<Chambre> CreateAsync(Chambre chambre)
        {
            _context.Chambres.Add(chambre);
            await _context.SaveChangesAsync();
            return chambre;
        }

        public async Task<List<Chambre>> GetByHotelAsync(int hotelId)
        {
            return await _context.Chambres
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .ToListAsync();
        }

        // 🔥 disponibilité (très important)
        public async Task<List<Chambre>> GetAvailableAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            return await _context.Chambres
                .Where(c => c.HotelId == hotelId && c.IsActive)
                .Where(c => !c.Reservations.Any(r =>
                    r.Status != ReservationStatus.Cancelled &&
                    r.CheckIn < checkOut &&
                    r.CheckOut > checkIn
                ))
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var chambre = await _context.Chambres.FindAsync(id);
            if (chambre == null) return false;

            chambre.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Chambre>> GetAllAsync()
        {
            return await _context.Chambres.ToListAsync();
        }
    }
}