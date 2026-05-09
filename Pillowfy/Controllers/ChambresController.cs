using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Interfaces;
using Pillowfy.Models;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChambresController : ControllerBase
    {
        private readonly IChambreService _service;

        public ChambresController(IChambreService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var chambres = await _service.GetAllAsync();
            return Ok(chambres);
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetByHotel(int hotelId)
        {
            return Ok(await _service.GetByHotelAsync(hotelId));
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            return Ok(await _service.GetAvailableAsync(hotelId, checkIn, checkOut));
        }

        [HttpPost]
        public async Task<IActionResult> Create(ChambreCreateDto dto)
        {
            var chambre = new Chambre
            {
                Name = dto.Name,
                Description = dto.Description,
                PricePerNight = dto.PricePerNight,
                Capacity = dto.Capacity,
                HotelId = dto.HotelId,
                IsActive = true
            };

            var result = await _service.CreateAsync(chambre);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }
    }
}