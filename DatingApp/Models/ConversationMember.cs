using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class ConversationMember : BaseEntity
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public virtual DatingappUser User { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}
