using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs;
using Pillowfy.Interfaces;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service)
        {
            _service = service;
        }

        // ── Créer une réservation ──────────────────────────────
        // POST /api/reservations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var result = await _service.CreateAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Liste des réservations du client connecté ──────────
        // GET /api/client/reservations  (attendu par la vue)
        [HttpGet("/api/client/reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _service.GetUserReservationsAsync(userId);
            return Ok(result);
        }

        // ── Annuler une réservation ────────────────────────────
        // PUT /api/reservations/{id}/cancel  (attendu par la vue)
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var result = await _service.CancelAsync(id, userId);
                if (!result) return NotFound(new { message = "Réservation introuvable." });

                return Ok(new { message = "Réservation annulée avec succès." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ── Télécharger la facture PDF ─────────────────────────
        // GET /api/reservations/{id}/invoice  (attendu par la vue)
        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null) return Unauthorized();

                var pdf = await _service.GenerateInvoiceAsync(id, userId);
                if (pdf == null) return NotFound(new { message = "Facture introuvable." });

                return File(pdf, "application/pdf", $"facture-{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var reservations = await _service.GetUserReservationsAsync(userId);
            var r = reservations.FirstOrDefault(x => x.Id == id);
            if (r == null) return NotFound();
            return Ok(r);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ReservationUpdateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                var result = await _service.UpdateAsync(id, userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        

        // ── Helper ─────────────────────────────────────────────
        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}