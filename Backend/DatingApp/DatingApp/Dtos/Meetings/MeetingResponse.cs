using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Meetings
{
    public class MeetingResponse
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int ConversationId { get; set; }
        public string LocationName { get; set; } = null!;
        public string LocationAddress { get; set; } = null!;
        public DateTime MeetingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> UserIds { get; set; } = new();
        public List<UserResponse> Users { get; set; } = new();
        public List<string> InvitedUserIds { get; set; } = new();
        public List<UserResponse> InvitedUsers { get; set; } = new();
    }
}
