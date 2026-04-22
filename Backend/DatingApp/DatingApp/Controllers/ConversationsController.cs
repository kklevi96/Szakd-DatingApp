using DatingApp.Dtos.Conversations;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IUserService _userService;

        public ConversationsController(IConversationService conversationService, IUserService userService)
        {
            _conversationService = conversationService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateConversationRequest request)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _conversationService.CreateConversationAsync(currentUserId, request);
            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMy()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _conversationService.GetMyConversationsAsync(currentUserId);
            if (result != null)
                return Ok(result);

            return BadRequest();

        }
    }
}