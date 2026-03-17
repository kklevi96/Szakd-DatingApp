using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Message : BaseEntity
    {
        //[ForeignKey(nameof(Conversation))]
        [Required]
        public int ConversationId { get; set; }

        //[ForeignKey(nameof(DatingappUser))]
        [Required]
        public string SenderUserId { get; set; }

        [NotMapped]
        public virtual DatingappUser Sender { get; set; }

        public string Content { get; set; }

        [NotMapped]
        public virtual Conversation Conversation { get; set; }
    }
}
