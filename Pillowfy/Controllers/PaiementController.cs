using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Services;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaiementController : ControllerBase
    {
        private readonly PaiementService _paiementService;

        public PaiementController(PaiementService paiementService)
        {
            _paiementService = paiementService;
        }

        [HttpPost]
        public async Task<IActionResult> Pay([FromBody] PaiementCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _paiementService.ProcessPaymentAsync(dto, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            { return Forbid(ex.Message); }
            catch (Exception ex)
            { return BadRequest(new { message = ex.Message }); }
        }

        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetByReservation(int reservationId)
        {
            var result = await _paiementService
                .GetPaymentByReservationAsync(reservationId);
            if (result == null)
                return NotFound(new { message = "Aucun paiement trouvé." });
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _paiementService.GetAllPaymentsAsync());
        }

        [HttpPost("remboursement/{paiementId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund(int paiementId)
        {
            var result = await _paiementService.RefundPaymentAsync(paiementId);
            if (!result)
                return NotFound(new { message = "Paiement introuvable." });
            return Ok(new { message = "Remboursement effectué." });
        }
        [HttpGet("mes-paiements")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var result = await _paiementService.GetPaymentsByUserAsync(userId);

            return Ok(result);
        }
    }
}