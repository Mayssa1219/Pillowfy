using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Factory;
using Pillowfy.Interfaces;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChambresController : ControllerBase
    {
        private readonly IChambreService _service;
        private readonly IChambreFactory _factory;

        public ChambresController(IChambreService service, IChambreFactory factory)
        {
            _service = service;
            _factory = factory;
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
            var chambre = _factory.Create(dto);  // ← plus de new Chambre ici
            var result = await _service.CreateAsync(chambre);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _service.DeleteAsync(id));
        }

        [HttpGet("disponibles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisponibles()
        {
            var chambres = await _service.GetAllAsync();
            return Ok(chambres);
        }
    }
}