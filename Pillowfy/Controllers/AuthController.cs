using Microsoft.AspNetCore.Mvc;
using Pillowfy.DTOs.Auth;
using Pillowfy.Services;

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
			return result.Success ? Ok(result) : Unauthorized(result);
		}

		[HttpPost("assign-role")]
		public async Task<ActionResult<AuthResponseDto>> AssignRole([FromBody] AssignRoleDto model)
		{
			var result = await _authService.AssignRoleAsync(model.UserId, model.Role);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}

	public class AssignRoleDto
	{
		public string UserId { get; set; }
		public string Role { get; set; }
	}
}