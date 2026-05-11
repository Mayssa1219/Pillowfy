using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pillowfy.Data;
using Pillowfy.DTOs;
using Pillowfy.Models;
using Pillowfy.Services;
using System.Security.Claims;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Owner")]
[ApiController]
[Route("api/owner")]
public class OwnerApiController : ControllerBase
{
    private readonly OwnerDashboardService _service;
    private readonly IWebHostEnvironment _env;
    private readonly OwnerPaiementService _paiementsService;
    private readonly OwnerRevenusService _revenusService;
    private readonly OwnerAvisService _avisService;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PilloWfyDbContext _context;

    public OwnerApiController(
        OwnerDashboardService service,
        IWebHostEnvironment env,
        OwnerPaiementService paiementsService,
        OwnerRevenusService revenusService,
        OwnerAvisService avisService,
        UserManager<ApplicationUser> userManager,
        PilloWfyDbContext context)
    {
        _service = service;
        _env = env;
        _paiementsService = paiementsService;
        _revenusService = revenusService;
        _avisService = avisService;

        _userManager = userManager;
        _context = context;
    }

    // ─────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────

    private string? GetOwnerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<string> SaveImageAsync(IFormFile image, string folder = "hotels")
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);

        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, fileName);

        using var stream = System.IO.File.Create(filePath);

        await image.CopyToAsync(stream);

        return $"/uploads/{folder}/{fileName}";
    }

    // ─────────────────────────────────────────────
    // PROFILE
    // ─────────────────────────────────────────────

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            phoneNumber = user.PhoneNumber,
            createdAt = user.CreatedAt
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateOwnerProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            message = "Profil mis à jour"
        });
    }

    [HttpPut("profile/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangeOwnerPasswordDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword
        );

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Mot de passe actuel incorrect"
            });
        }

        return Ok(new
        {
            message = "Mot de passe changé"
        });
    }

    [HttpPost("profile/deactivate")]
    public async Task<IActionResult> DeactivateAccount()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        user.IsActive = false;

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            message = "Compte désactivé"
        });
    }

    // ─────────────────────────────────────────────
    // DASHBOARD
    // ─────────────────────────────────────────────

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var data = await _service.GetDashboardAsync(ownerId);

        return Ok(data);
    }

    [HttpGet("hotels/occupancy")]
    public async Task<IActionResult> GetOccupancy()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var data = await _service.GetOccupancyAsync(ownerId);

        return Ok(data);
    }

    // ─────────────────────────────────────────────
    // HOTELS
    // ─────────────────────────────────────────────

    [HttpGet("hotels")]
    public async Task<IActionResult> GetHotels()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var data = await _service.GetHotelsAsync(ownerId);

        return Ok(data);
    }

    [HttpPost("hotels")]
    public async Task<IActionResult> CreateHotel(
        [FromForm] HotelCreateDto dto,
        IFormFile? image)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.City) ||
            string.IsNullOrWhiteSpace(dto.Country))
        {
            return BadRequest(new
            {
                message = "Nom, ville et pays sont obligatoires."
            });
        }

        if (image != null)
        {
            dto.ImageUrl = await SaveImageAsync(image);
        }

        var result = await _service.CreateHotelAsync(ownerId, dto);

        return Ok(result);
    }

    [HttpPut("hotels/{id}")]
    public async Task<IActionResult> UpdateHotel(
        int id,
        [FromForm] HotelCreateDto dto,
        IFormFile? image)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        if (image != null)
        {
            dto.ImageUrl = await SaveImageAsync(image);
        }

        var result = await _service.UpdateHotelAsync(ownerId, id, dto);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Hôtel introuvable."
            });
        }

        return Ok(result);
    }

    [HttpDelete("hotels/{id}")]
    public async Task<IActionResult> DeleteHotel(int id)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var ok = await _service.DeleteHotelAsync(ownerId, id);

        if (!ok)
        {
            return NotFound(new
            {
                message = "Hôtel introuvable."
            });
        }

        return Ok(new
        {
            message = "Hôtel supprimé."
        });
    }

    // ─────────────────────────────────────────────
    // CHAMBRES
    // ─────────────────────────────────────────────

    [HttpGet("chambres")]
    public async Task<IActionResult> GetChambres()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var data = await _service.GetChambresAsync(ownerId);

        return Ok(data);
    }

    [HttpPost("chambres")]
    public async Task<IActionResult> CreateChambre(
        [FromForm] ChambreCreateDto dto,
        IFormFile? image)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.Name) ||
            dto.HotelId == 0 ||
            dto.PricePerNight <= 0)
        {
            return BadRequest(new
            {
                message = "Nom, hôtel et prix sont obligatoires."
            });
        }

        string? imageUrl = null;

        if (image != null)
        {
            imageUrl = await SaveImageAsync(image, "chambres");
        }

        var result = await _service.CreateChambreAsync(ownerId, dto, imageUrl);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Hôtel introuvable."
            });
        }

        return Ok(result);
    }

    [HttpPut("chambres/{id}")]
    public async Task<IActionResult> UpdateChambre(
        int id,
        [FromForm] ChambreCreateDto dto,
        IFormFile? image)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        string? imageUrl = null;

        if (image != null)
        {
            imageUrl = await SaveImageAsync(image, "chambres");
        }

        var result = await _service.UpdateChambreAsync(
            ownerId,
            id,
            dto,
            imageUrl);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Chambre introuvable."
            });
        }

        return Ok(result);
    }

    [HttpDelete("chambres/{id}")]
    public async Task<IActionResult> DeleteChambre(int id)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var ok = await _service.DeleteChambreAsync(ownerId, id);

        if (!ok)
        {
            return NotFound(new
            {
                message = "Chambre introuvable."
            });
        }

        return Ok(new
        {
            message = "Chambre supprimée."
        });
    }

    // ─────────────────────────────────────────────
    // RESERVATIONS
    // ─────────────────────────────────────────────

    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var data = await _service.GetReservationsAsync(ownerId);

        return Ok(data);
    }

    [HttpPatch("reservations/{id}/status")]
    public async Task<IActionResult> UpdateReservationStatus(
        int id,
        [FromBody] UpdateStatusDto dto)
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var ok = await _service.UpdateReservationStatusAsync(
            ownerId,
            id,
            dto.Status);

        if (!ok)
        {
            return NotFound(new
            {
                message = "Réservation introuvable."
            });
        }

        return Ok(new
        {
            message = "Statut mis à jour."
        });
    }

    // ─────────────────────────────────────────────
    // PAIEMENTS
    // ─────────────────────────────────────────────

    [HttpGet("paiements")]
    public async Task<IActionResult> GetPaiements()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var result = await _paiementsService.GetPaiementsAsync(ownerId);

        return Ok(result);
    }

    // ─────────────────────────────────────────────
    // REVENUS
    // ─────────────────────────────────────────────

    [HttpGet("revenus")]
    public async Task<IActionResult> GetRevenus()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var result = await _revenusService.GetRevenusAsync(ownerId);

        return Ok(result);
    }

    // ─────────────────────────────────────────────
    // AVIS
    // ─────────────────────────────────────────────

    [HttpGet("avis")]
    public async Task<IActionResult> GetAvis()
    {
        var ownerId = GetOwnerId();

        if (string.IsNullOrEmpty(ownerId))
            return Unauthorized();

        var result = await _avisService.GetAvisAsync(ownerId);

        return Ok(result);
    }
}