using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class DatingappUser : IdentityUser
    {
        public DateTime BirthDay { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool InterestedinWoman { get; set; }

        public bool InterestedinMan { get; set; }

        public string Gender { get; set; }

        public int MBTICode { get; set; }

        [NotMapped]
        public virtual ICollection<MeetingAttendance> MeetingAttendances { get; set; }

        [NotMapped]
        public virtual ICollection<HobbyUser> HobbyUsers { get; set; }

        [NotMapped]
        public virtual ICollection<ConversationMember> ConversationMembers { get; set; }

        [NotMapped]
        public virtual ICollection<Message> Messages { get; set; }

        [NotMapped]
        public virtual ICollection<Compatibility> CompatibilitiesAsUserA { get; set; }

        [NotMapped]
        public virtual ICollection<Compatibility> CompatibilitiesAsUserB { get; set; }

    }
}
