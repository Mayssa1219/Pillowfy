using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Interfaces;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ReservationCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("UserId introuvable dans le token");

            var result = await _service.CreateAsync(userId, dto);
            return Ok(result);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserReservations()
        {
            var userId = "test-user";
            return Ok(await _service.GetUserReservationsAsync(userId));
        }

        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = "test-user";
            return Ok(await _service.CancelAsync(id, userId));
        }
        

    }
}