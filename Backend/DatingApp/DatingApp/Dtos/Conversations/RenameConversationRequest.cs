using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dtos.Conversations
{
    public class RenameConversationRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
