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

        [HttpPost("check/{otherUserId}")]
        public async Task<IActionResult> CheckOrCreate(string otherUserId)
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId(User);
                var result = await _compatibilityService.CheckOrCreateCompatibilityAsync(currentUserId, otherUserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyCompatibilities()
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId(User);
                var result = await _compatibilityService.GetMyCompatibilitiesAsync(currentUserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("generate-my")]
        public async Task<IActionResult> GenerateMy()
        {
            try
            {
                var currentUserId = _userService.GetCurrentUserId(User);
                var createdCount = await _compatibilityService.GenerateMyCompatibilitiesAsync(currentUserId);
                return Ok(new { createdCount });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}