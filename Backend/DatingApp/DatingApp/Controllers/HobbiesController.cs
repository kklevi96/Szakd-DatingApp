using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HobbiesController : ControllerBase
    {
        private readonly IHobbyService _hobbyService;
        private readonly IUserService _userService;

        public HobbiesController(IHobbyService hobbyService, IUserService userService)
        {
            _hobbyService = hobbyService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var hobbies = await _hobbyService.GetAllHobbiesAsync();
            return Ok(hobbies);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyHobbies()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var hobbies = await _hobbyService.GetMyHobbiesAsync(currentUserId);

            return Ok(hobbies);
        }

        [HttpPut("{id:int}/add")]
        public async Task<IActionResult> Add(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _hobbyService.AddHobbyAsync(currentUserId, id);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpDelete("{id:int}/remove")]
        public async Task<IActionResult> Remove(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _hobbyService.RemoveHobbyAsync(currentUserId, id);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
