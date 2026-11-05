using DatingApp.Dtos.Meetings;

namespace DatingApp.Services
{
    public interface IMeetingService
    {
        Task<MeetingResponse> CreateMeetingAsync(string currentUserId, CreateMeetingRequest request);
        Task<MeetingResponse?> GetMeetingByIdAsync(int id, string currentUserId);
        Task<List<MeetingResponse>> GetMyMeetingsAsync(string currentUserId);
        Task<List<MeetingResponse>> GetMyInvitationsAsync(string currentUserId);
        Task<bool> JoinMeetingAsync(int meetingId, string currentUserId);
        Task<bool> LeaveMeetingAsync(int meetingId, string currentUserId);
        Task<bool> DeleteMeetingAsync(int meetingId, string currentUserId);
    }
}
