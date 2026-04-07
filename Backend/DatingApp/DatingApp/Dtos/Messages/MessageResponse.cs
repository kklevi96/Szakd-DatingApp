using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Messages
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public string SenderUserId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public UserResponse? Sender { get; set; }
    }
}