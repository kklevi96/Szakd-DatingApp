using DatingApp.Dtos.Meetings;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IUserService _userService;

        public MeetingsController(IMeetingService meetingService, IUserService userService)
        {
            _meetingService = meetingService;
            _userService = userService;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyMeetings()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var meetings = await _meetingService.GetMyMeetingsAsync(currentUserId);

            return Ok(meetings);
        }

        [HttpGet("conversation/{conversationId:int}")]
        public async Task<IActionResult> GetByConversation(int conversationId)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            try
            {
                var meetings = await _meetingService.GetMeetingsByConversationAsync(
                    conversationId, currentUserId);

                return Ok(meetings);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("invitations")]
        public async Task<IActionResult> GetMyInvitations()
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var meetings = await _meetingService.GetMyInvitationsAsync(currentUserId);

            return Ok(meetings);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var meeting = await _meetingService.GetMeetingByIdAsync(id, currentUserId);

            if (meeting == null)
            {
                return NotFound();
            }

            return Ok(meeting);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMeetingRequest request)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            try
            {
                var meeting = await _meetingService.CreateMeetingAsync(currentUserId, request);

                return Ok(meeting);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }


        [HttpPut("{id:int}/join")]
        public async Task<IActionResult> Join(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            try
            {
                var result = await _meetingService.JoinMeetingAsync(id, currentUserId);

                if (!result)
                {
                    return NotFound();
                }
                else
                {
                    return Ok();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpDelete("{id:int}/leave")]
        public async Task<IActionResult> Leave(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);
            var result = await _meetingService.LeaveMeetingAsync(id, currentUserId);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }

        /*
         * admin user
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userService.GetCurrentUserId(User);

            try
            {
                var result = await _meetingService.DeleteMeetingAsync(id, currentUserId);

                if (!result)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
        */
    }
}
