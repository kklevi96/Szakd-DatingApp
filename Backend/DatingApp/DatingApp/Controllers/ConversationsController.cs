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

            try
            {
                var result = await _conversationService
                    .CreateConversationAsync(currentUserId, request);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("{conversationId}/name")]
        public async Task<IActionResult> Rename(int conversationId, [FromBody] RenameConversationRequest request)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            try
            {
                return Ok(await _conversationService.RenameConversationAsync(currentUserId, conversationId, request));
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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