namespace DatingApp.Dtos.Conversations
{
    public class CreateConversationRequest
    {
        public bool IsGroup { get; set; }

        public List<string> OtherUserIds { get; set; } = new();

        public string? Name { get; set; }
    }
}