using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public ClientDashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
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
    }
}