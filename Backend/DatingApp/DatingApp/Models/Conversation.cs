using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Conversation : BaseEntity
    {
        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<ConversationMember> ConversationMembers { get; set; }
    }
}
