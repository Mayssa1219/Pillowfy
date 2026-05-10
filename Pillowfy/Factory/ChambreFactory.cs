using Pillowfy.DTOs;
using Pillowfy.Models;
using Pillowfy.Interfaces;
namespace Pillowfy.Factory
{
    public class ChambreFactory : IChambreFactory
    {
        public Chambre Create(ChambreCreateDto dto)
        {
            return new Chambre
            {
                Name = dto.Name,
                Description = dto.Description,
                PricePerNight = dto.PricePerNight,
                Capacity = dto.Capacity,
                HotelId = dto.HotelId,
                IsActive = true
            };
        }
    }
}
