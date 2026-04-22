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

            if (result != null)
                return Ok(result);

            return BadRequest();

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (result != null)
                return Ok(result);

            return Unauthorized();
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
    }
}