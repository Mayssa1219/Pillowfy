using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs.Auth;
using Pillowfy.Services;
using System.Security.Claims;

namespace Pillowfy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.RegisterAsync(model);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);
            if (!result.Success)
                return Unauthorized(result);

            // ✅ Créer le cookie MVC pour que [Authorize] fonctionne
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, result.User!.Id),
        new(ClaimTypes.Name,           $"{result.User.FirstName} {result.User.LastName}"),
        new(ClaimTypes.Email,          result.User.Email!),
        new(ClaimTypes.Role,           result.Role!)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24) }
            );

            return Ok(result);
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult<AuthResponseDto>> AssignRole([FromBody] AssignRoleDto model)
        {
            var result = await _authService.AssignRoleAsync(model.UserId, model.Role);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.ForgotPasswordAsync(model.Email);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authService.ResetPasswordAsync(model);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    // ← Reste DANS le namespace
    public class AssignRoleDto
    {
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}