using DatingApp.Dtos.Users;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var result = await _userService.GetCurrentUserAsync(User);

            if (result == null)
                return Unauthorized();

            return Ok(result);
        }

        [HttpPatch("search-preferences")]
        public async Task<IActionResult> UpdateSearchPreferences(
            SearchPreferencesRequest request)
        {
            var result = await _userService.UpdateSearchPreferencesAsync(
                User, request);

            if (result == null)
                return Unauthorized();

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }
    }
}
