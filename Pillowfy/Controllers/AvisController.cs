using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Services;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvisController : ControllerBase
    {
        private readonly AvisService _avisService;

        public AvisController(AvisService avisService)
        {
            _avisService = avisService;
        }

        [HttpGet("hotel/{hotelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByHotel(int hotelId)
        {
            return Ok(await _avisService.GetAvisByHotelAsync(hotelId));
        }

        [HttpGet("mes-avis")]
        [Authorize]
        public async Task<IActionResult> GetMyAvis()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            return Ok(await _avisService.GetAvisByUserAsync(userId));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAvis([FromBody] AvisCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                return Ok(await _avisService.AddAvisAsync(dto, userId));
            }
            catch (Exception ex)
            { return BadRequest(new { message = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var isAdmin = User.IsInRole("Admin");
                var result = await _avisService.DeleteAvisAsync(id, userId, isAdmin);
                if (!result)
                    return NotFound(new { message = "Avis introuvable." });
                return Ok(new { message = "Avis supprimé." });
            }
            catch (UnauthorizedAccessException ex)
            { return Forbid(ex.Message); }
        }
    }
}