using DatingApp.Dtos.Auth;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
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
        public async Task<IActionResult> Register([FromBody] AuthRegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result.Errors);

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _authService.GetCurrentUserAsync(User);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [Authorize]
        [HttpPatch("DelegateAdmin")]
        public async Task<IActionResult> DelegateAdmin([FromBody] DelegateAdminRequest request)
        {
            var result = await _authService.DelegateAdminAsync(User, request);
            if (result == null)
                return Unauthorized();

            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = string.Join(" ", result.Errors.Select(error => error.Description))
                });

            return Ok(new { message = "Admin role granted. Please sign in again." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admin")]
        public IActionResult Admin()
        {
            return Ok(new { message = "Admin access confirmed by the backend." });
        }
    }
}
