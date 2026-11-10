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
            var currentUserId = _userService.GetCurrentUserId(User);

            var result = await _compatibilityService
                .CheckOrCreateCompatibilityAsync(currentUserId, otherUserId);

            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpPost("my-dating")]
        public async Task<IActionResult> GetMyDating()
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            await _compatibilityService.GenerateMyCompatibilitiesAsync(currentUserId);

            var result = await _compatibilityService
                .GetMyCompatibilitiesAsync(currentUserId);

            return Ok(result);
        }

        [HttpGet("friendship")]
        public async Task<IActionResult> GetFriendshipUsers()
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            var result = await _compatibilityService
                .GetFriendshipUsersAsync(currentUserId);

            return Ok(result);
        }
    }
}