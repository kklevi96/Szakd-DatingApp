using DatingApp.Dtos.Messages;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessagesController(IMessageService messageService, IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _messageService.SendMessageAsync(currentUserId, request);
            if (result != null)
                return Ok(result);

            return BadRequest();
        }

        [HttpGet("conversation/{conversationId}")]
        public async Task<IActionResult> GetByConversation(int conversationId)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _messageService.GetMessagesByConversationAsync(currentUserId, conversationId);
            if (result != null)
                return Ok(result);

            return BadRequest();
        }
    }
}