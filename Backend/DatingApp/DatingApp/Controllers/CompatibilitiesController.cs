using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CompatibilitiesController : ControllerBase
    {
        private readonly ICompatibilityService _compatibilityService;
        private readonly IUserService _userService;

        public CompatibilitiesController(ICompatibilityService compatibilityService, IUserService userService)
        {
            _compatibilityService = compatibilityService;
            _userService = userService;
        }

        [Authorize]
        [HttpPost("check/{otherUserId}")]
        public async Task<IActionResult> CheckOrCreate(string otherUserId)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            var result = await _compatibilityService
                .CheckOrCreateCompatibilityAsync(currentUserId, otherUserId);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyCompatibilities()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _compatibilityService.GetMyCompatibilitiesAsync(currentUserId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("generate-my")]
        public async Task<IActionResult> GenerateMy()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var createdCount = await _compatibilityService.GenerateMyCompatibilitiesAsync(currentUserId);

            if (createdCount >= 0)
                return Ok(new { createdCount });

            return BadRequest();
        }
    }
}