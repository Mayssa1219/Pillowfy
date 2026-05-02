using Pillowfy.Models;

namespace Pillowfy.Interfaces
{
    public interface IChambreService
    {
        Task<Chambre> CreateAsync(Chambre chambre);
        Task<List<Chambre>> GetByHotelAsync(int hotelId);
        Task<List<Chambre>> GetAvailableAsync(int hotelId, DateTime checkIn, DateTime checkOut);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Chambre>> GetAllAsync();
    }
}