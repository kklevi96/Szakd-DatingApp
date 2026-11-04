using DatingApp.Data;
using DatingApp.Dtos.Meetings;
using DatingApp.Dtos.Users;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly ApplicationDbContext _context;

        public MeetingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MeetingResponse> CreateMeetingAsync(string currentUserId, CreateMeetingRequest request)
        {
            var locationExists = await _context.Locations
                .AnyAsync(l => l.Id == request.LocationId);

            if (!locationExists)
            {
                throw new KeyNotFoundException("Location not found.");
            }

            var meeting = new Meeting
            {
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
            await _context.SaveChangesAsync();

            return await GetMeetingResponseAsync(meeting.Id);
        }

        public async Task<List<MeetingResponse>> GetAllMeetingsAsync()
        {
            var meetings = await MeetingQuery()
                .OrderBy(m => m.MeetingDate)
                .ToListAsync();

            return meetings.Select(MapMeeting).ToList();
        }

        public async Task<MeetingResponse?> GetMeetingByIdAsync(int id)
        {
            var meeting = await MeetingQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            return meeting == null ? null : MapMeeting(meeting);
        }

        public async Task<List<MeetingResponse>> GetMyMeetingsAsync(string currentUserId)
        {
            var meetings = await MeetingQuery()
                .Where(m => m.MeetingAttendances.Any(ma => ma.UserId == currentUserId))
                .OrderBy(m => m.MeetingDate)
                .ToListAsync();

            return meetings.Select(MapMeeting).ToList();
        }

        public async Task<bool> JoinMeetingAsync(int meetingId, string currentUserId)
        {
            var meeting = await _context.Meetings
                .Include(m => m.MeetingAttendances)
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
            {
                return false;
            }

            if (meeting.MeetingAttendances.Any(ma => ma.UserId == currentUserId))
            {
                return true;
            }

            meeting.MeetingAttendances.Add(new MeetingAttendance
            {
                UserId = currentUserId
            });

            await _context.SaveChangesAsync();

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

            return MapMeeting(meeting);
        }

        private static MeetingResponse MapMeeting(Meeting meeting)
        {
            return new MeetingResponse
            {
                Id = meeting.Id,
                LocationId = meeting.LocationId,
                LocationName = meeting.Location.Name,
                LocationAddress = meeting.Location.Address,
                MeetingDate = meeting.MeetingDate,
                CreatedAt = meeting.CreatedAt,

                UserIds = meeting.MeetingAttendances
                    .Select(ma => ma.UserId)
                    .ToList(),

                Users = meeting.MeetingAttendances
                    .Select(ma => MapUser(ma.User))
                    .ToList()
            };
        }

        private static UserResponse MapUser(DatingappUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                InterestedinWoman = user.InterestedinWoman,
                InterestedinMan = user.InterestedinMan,
                Gender = user.Gender,
                MBTICode = user.MBTICode
            };
        }
    }
}