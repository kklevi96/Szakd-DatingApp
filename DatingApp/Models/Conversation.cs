using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Conversation : BaseEntity
    {
        [NotMapped]
        public virtual ICollection<Message> Messages { get; set; }

        [NotMapped]
        public virtual ICollection<ConversationMember> ConversationMembers { get; set; }
    }
}
