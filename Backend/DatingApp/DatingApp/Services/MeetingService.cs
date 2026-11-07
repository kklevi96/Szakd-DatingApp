using DatingApp.Data;
using DatingApp.Dtos.Conversations;
using DatingApp.Dtos.Meetings;
using DatingApp.Mappers;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly DatingAppDbContext _context;
        private readonly IConversationService _conversationService;

        public MeetingService(DatingAppDbContext context, IConversationService conversationService)
        {
            _context = context;
            _conversationService = conversationService;
        }

        public async Task<MeetingResponse> CreateMeetingAsync(string currentUserId, CreateMeetingRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OtherUserId))
            {
                throw new ArgumentException("An invited user is required.");
            }

            if (currentUserId == request.OtherUserId)
            {
                throw new ArgumentException("You cannot invite yourself.");
            }

            var invitedUserExists = await _context.Users
                .AnyAsync(user => user.Id == request.OtherUserId);

            if (!invitedUserExists)
            {
                throw new KeyNotFoundException("Invited user not found.");
            }

            var locationExists = await _context.Locations
                .AnyAsync(l => l.Id == request.LocationId);

            if (!locationExists)
            {
                throw new KeyNotFoundException("Location not found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var conversation = await _conversationService.CreateConversationAsync(
                currentUserId,
                new CreateConversationRequest
                {
                    IsGroup = false,
                    OtherUserIds = new List<string> { request.OtherUserId }
                });

            var meeting = new Meeting
            {
                ConversationId = conversation.Id,
                LocationId = request.LocationId,
                MeetingDate = request.MeetingDate,
                MeetingAttendances = new List<MeetingAttendance>
                {
                    new MeetingAttendance
                    {
                        UserId = currentUserId
                    }
                }
            };

            _context.Meetings.Add(meeting);

            _context.MeetingInvitations.Add(new MeetingInvitation
            {
                Meeting = meeting,
                UserId = request.OtherUserId
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetMeetingResponseAsync(meeting.Id);
        }

        public async Task<MeetingResponse?> GetMeetingByIdAsync(int id, string currentUserId)
        {
            var meeting = await MeetingQuery()
                .Where(meeting =>
                    meeting.MeetingAttendances.Any(attendance => attendance.UserId == currentUserId) ||
                    _context.MeetingInvitations.Any(invitation =>
                        invitation.MeetingId == meeting.Id && invitation.UserId == currentUserId))
                .FirstOrDefaultAsync(m => m.Id == id);

            return meeting == null ? null : MeetingMapper.ToResponse(meeting);
        }

        public async Task<List<MeetingResponse>> GetMyMeetingsAsync(string currentUserId)
        {
            var meetings = await MeetingQuery()
                .Where(m => m.MeetingAttendances.Any(ma => ma.UserId == currentUserId))
                .OrderBy(m => m.MeetingDate)
                .ToListAsync();

            return meetings.Select(MeetingMapper.ToResponse).ToList();
        }

        public async Task<List<MeetingResponse>> GetMyInvitationsAsync(string currentUserId)
        {
            var meetings = await MeetingQuery()
                .Where(meeting => _context.MeetingInvitations.Any(invitation =>
                    invitation.MeetingId == meeting.Id && invitation.UserId == currentUserId))
                .OrderBy(meeting => meeting.MeetingDate)
                .ToListAsync();

            return meetings.Select(MeetingMapper.ToResponse).ToList();
        }

        public async Task<bool> JoinMeetingAsync(int meetingId, string currentUserId)
        {
            var meetingExists = await _context.Meetings
                .AnyAsync(meeting => meeting.Id == meetingId);

            if (!meetingExists)
            {
                return false;
            }

            var alreadyJoined = await _context.MeetingAttendances
                .AnyAsync(attendance =>
                    attendance.MeetingId == meetingId &&
                    attendance.UserId == currentUserId);

            if (alreadyJoined)
            {
                return true;
            }

            var invitation = await _context.MeetingInvitations
                .SingleOrDefaultAsync(invitation =>
                    invitation.MeetingId == meetingId &&
                    invitation.UserId == currentUserId);

            if (invitation == null)
            {
                var joinedMeanwhile = await _context.MeetingAttendances
                    .AnyAsync(attendance =>
                        attendance.MeetingId == meetingId && attendance.UserId == currentUserId);

                if (joinedMeanwhile)
                {
                    return true;
                }

                throw new UnauthorizedAccessException(
                    "You are not invited to this meeting.");
            }

            var newAttendance = new MeetingAttendance
            {
                MeetingId = meetingId,
                UserId = currentUserId
            };

            _context.MeetingAttendances.Add(newAttendance);
            _context.MeetingInvitations.Remove(invitation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _context.Entry(newAttendance).State = EntityState.Detached;
                _context.Entry(invitation).State = EntityState.Detached;

                var joinedMeanwhile = await _context.MeetingAttendances
                    .AnyAsync(existingAttendance =>
                        existingAttendance.MeetingId == meetingId &&
                        existingAttendance.UserId == currentUserId);

                if (joinedMeanwhile)
                {
                    return true;
                }

                var meetingStillExists = await _context.Meetings
                    .AnyAsync(meeting => meeting.Id == meetingId);

                if (!meetingStillExists)
                {
                    return false;
                }

                throw;
            }

            return true;
        }

        public async Task<bool> LeaveMeetingAsync(int meetingId, string currentUserId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.MeetingAttendances)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
            {
                return false;
            }

            var attendance = meeting.MeetingAttendances
                .FirstOrDefault(ma => ma.UserId == currentUserId);

            if (attendance == null)
            {
                return false;
            }

            if (meeting.MeetingAttendances.Count == 1)
            {
                _context.Meetings.Remove(meeting);
            }
            else
            {
                _context.MeetingAttendances.Remove(attendance);
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteMeetingAsync(int meetingId, string currentUserId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.MeetingAttendances)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
            {
                return false;
            }

            var isParticipant = meeting.MeetingAttendances
                .Any(ma => ma.UserId == currentUserId);

            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("You are not a participant of this meeting.");
            }

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return true;
        }

        private IQueryable<Meeting> MeetingQuery()
        {
            return _context.Meetings
                .Include(m => m.Location)
                .Include(m => m.MeetingAttendances)
                .ThenInclude(ma => ma.User);
        }

        private async Task<MeetingResponse> GetMeetingResponseAsync(int meetingId)
        {
            var meeting = await MeetingQuery()
                .FirstAsync(m => m.Id == meetingId);

            return MeetingMapper.ToResponse(meeting);
        }
    }
}
