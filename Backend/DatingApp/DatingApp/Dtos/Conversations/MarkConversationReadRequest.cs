using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dtos.Conversations
{
    public class MarkConversationReadRequest
    {
        [Range(1, int.MaxValue)]
        public int LastReadMessageId { get; set; }
    }
}
