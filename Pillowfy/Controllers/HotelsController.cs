using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs.Hotel;
using Pillowfy.Services;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<HotelDto>> Create([FromBody] CreateHotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _hotelService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<HotelDto>> GetById(int id)
        {
            try
            {
                var result = await _hotelService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<HotelDto>>> GetAll()
        {
            var result = await _hotelService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("owner/my-hotels")]
        public async Task<ActionResult<List<HotelDto>>> GetMyHotels()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _hotelService.GetByOwnerAsync(userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult<HotelDto>> Update(int id, [FromBody] CreateHotelDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                var result = await _hotelService.UpdateAsync(id, dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _hotelService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}