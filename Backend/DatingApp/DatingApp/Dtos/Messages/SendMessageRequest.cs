namespace DatingApp.Dtos.Messages
{
    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public string Content { get; set; } = null!;
    }
}