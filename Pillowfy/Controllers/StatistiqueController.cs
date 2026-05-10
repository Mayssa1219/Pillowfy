using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.Services;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StatistiqueController : ControllerBase
    {
        private readonly StatistiqueService _statistiqueService;

        public StatistiqueController(StatistiqueService statistiqueService)
        {
            _statistiqueService = statistiqueService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return Ok(await _statistiqueService.GetDashboardAsync());
        }

        [HttpGet("revenu-total")]
        public async Task<IActionResult> GetRevenu()
        {
            var revenu = await _statistiqueService.GetRevenuTotalAsync();
            return Ok(new { revenuTotal = revenu, devise = "TND" });
        }

        [HttpGet("taux-occupation")]
        public async Task<IActionResult> GetTaux()
        {
            var taux = await _statistiqueService.GetTauxOccupationAsync();
            return Ok(new { tauxOccupation = taux, unite = "%" });
        }
    }
}