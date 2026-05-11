using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Crmf;
using Pillowfy.DTOs;
using Pillowfy.Enums;
using Pillowfy.Interfaces;
using System.Security.Claims;

namespace Pillowfy.Controllers.Api
{
    [ApiController]
    [Route("api/client")]
    [Authorize]
    public class ClientDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;
        private readonly IProfileService _profileService;

        public ClientDashboardController(IDashboardService dashboard, IProfileService profileService)
        {
            _dashboard = dashboard;
            _profileService = profileService;
        }

        // GET /api/client/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();
            var data = await _dashboard.GetClientDashboardAsync(userId);
            return Ok(data);
        }

        // GET /api/client/hotels/occupancy
        [HttpGet("hotels/occupancy")]
        public async Task<IActionResult> GetOccupancy()
        {
            var data = await _dashboard.GetOccupancyAsync();
            return Ok(data);
        }

        // GET /api/client/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var profile = await _profileService.GetAsync(userId);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        // PUT /api/client/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                var result = await _profileService.UpdateAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/client/profile/password
        [HttpPut("profile/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                await _profileService.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Mot de passe mis à jour." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/client/profile/deactivate
        [HttpPost("profile/deactivate")]
        public async Task<IActionResult> Deactivate()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                await _profileService.DeactivateAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("points")]
        public async Task<IActionResult> GetPoints()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Réutiliser le dashboard qui calcule déjà les points
            var dash = await _dashboard.GetClientDashboardAsync(userId);

            var points = dash.PointsFidelite;
            var remiseMax = (points / 100) * 10;

            return Ok(new
            {
                points = points,
                remiseMax = remiseMax,
                pointsParPalier = 100,
                eurosParPalier = 10,
                paliers = points / 100
            });
        }

    }
}