using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Conversations
{
    public class ConversationResponse
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsGroup { get; set; }
        public string? Name { get; set; }
        public List<string> MemberUserIds { get; set; } = new();
        public List<UserResponse> Members { get; set; } = new();
    }
}