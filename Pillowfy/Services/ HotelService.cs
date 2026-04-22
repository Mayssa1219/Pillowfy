using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs.Hotel;
using Pillowfy.Models;

namespace Pillowfy.Services
{
	public interface IHotelService
	{
		Task<HotelDto> CreateAsync(CreateHotelDto dto, string ownerId);
		Task<HotelDto> GetByIdAsync(int id);
		Task<List<HotelDto>> GetAllAsync();
		Task<List<HotelDto>> GetByOwnerAsync(string ownerId);
		Task<HotelDto> UpdateAsync(int id, CreateHotelDto dto, string ownerId);
		Task<bool> DeleteAsync(int id, string ownerId);
	}

	public class HotelService : IHotelService
	{
		private readonly PilloWfyDbContext _context;

		public HotelService(PilloWfyDbContext context)
		{
			_context = context;
		}

		public async Task<HotelDto> CreateAsync(CreateHotelDto dto, string ownerId)
		{
			try
			{
				var hotel = new Hotel
				{
					Name = dto.Name,
					Description = dto.Description,
					Address = dto.Address,
					City = dto.City,
					Country = dto.Country,
					OwnerId = ownerId,
					CreatedAt = DateTime.UtcNow,
					IsActive = true
				};

				_context.Hotels.Add(hotel);
				await _context.SaveChangesAsync();

				return MapToDto(hotel);
			}
			catch (Exception ex)
			{
				throw new Exception($"Erreur lors de la création de l'hôtel: {ex.Message}");
			}
		}

		public async Task<HotelDto> GetByIdAsync(int id)
		{
			var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id && h.IsActive);
			if (hotel == null)
				throw new Exception("Hôtel non trouvé.");

			return MapToDto(hotel);
		}

		public async Task<List<HotelDto>> GetAllAsync()
		{
			var hotels = await _context.Hotels
				.Where(h => h.IsActive)
				.ToListAsync();

			return hotels.Select(MapToDto).ToList();
		}

		public async Task<List<HotelDto>> GetByOwnerAsync(string ownerId)
		{
			var hotels = await _context.Hotels
				.Where(h => h.OwnerId == ownerId && h.IsActive)
				.ToListAsync();

			return hotels.Select(MapToDto).ToList();
		}

		public async Task<HotelDto> UpdateAsync(int id, CreateHotelDto dto, string ownerId)
		{
			var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id && h.OwnerId == ownerId);
			if (hotel == null)
				throw new Exception("Hôtel non trouvé ou vous n'avez pas les permissions.");

			hotel.Name = dto.Name;
			hotel.Description = dto.Description;
			hotel.Address = dto.Address;
			hotel.City = dto.City;
			hotel.Country = dto.Country;
			hotel.UpdatedAt = DateTime.UtcNow;

			_context.Hotels.Update(hotel);
			await _context.SaveChangesAsync();

			return MapToDto(hotel);
		}

		public async Task<bool> DeleteAsync(int id, string ownerId)
		{
			var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id && h.OwnerId == ownerId);
			if (hotel == null)
				throw new Exception("Hôtel non trouvé ou vous n'avez pas les permissions.");

			hotel.IsActive = false;
			hotel.UpdatedAt = DateTime.UtcNow;

			_context.Hotels.Update(hotel);
			await _context.SaveChangesAsync();

			return true;
		}

		private HotelDto MapToDto(Hotel hotel)
		{
			return new HotelDto
			{
				Id = hotel.Id,
				Name = hotel.Name,
				Description = hotel.Description,
				Address = hotel.Address,
				City = hotel.City,
				Country = hotel.Country,
				Rating = hotel.Rating,
				IsActive = hotel.IsActive,
				CreatedAt = hotel.CreatedAt,
				UpdatedAt = hotel.UpdatedAt
			};
		}
	}
}