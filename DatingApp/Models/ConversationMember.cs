using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class ConversationMember : BaseEntity
    {
        //[ForeignKey(nameof(Conversation))]
        [Required]
        public int ConversationId { get; set; }

        //[ForeignKey(nameof(DatingappUser))]
        [Required]
        public string UserId { get; set; } = null!;

        [NotMapped]
        public virtual DatingappUser User { get; set; }

        [NotMapped]
        public virtual Conversation Conversation { get; set; }
    }
}
