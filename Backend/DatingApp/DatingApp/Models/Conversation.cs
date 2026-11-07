using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Conversation : BaseEntity
    {
        public bool IsGroup { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        public virtual ICollection<Message> Messages { get; set; }

        public virtual ICollection<ConversationMember> ConversationMembers { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    }
}
