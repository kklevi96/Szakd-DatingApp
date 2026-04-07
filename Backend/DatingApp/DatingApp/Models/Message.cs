using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Message : BaseEntity
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        public string SenderUserId { get; set; }

        public virtual DatingappUser Sender { get; set; }

        public string Content { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}
