using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Meetings
{
    public class MeetingResponse
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> UserIds { get; set; } = new();
        public List<UserResponse> Users { get; set; } = new();
    }
}