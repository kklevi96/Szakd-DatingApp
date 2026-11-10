namespace DatingApp.Models
{
    public class Meeting : BaseEntity
    {
        public int LocationId { get; set; }
        public DateTime MeetingDate { get; set; }

        public virtual Location Location { get; set; } = null!;

        public virtual ICollection<MeetingAttendance> MeetingAttendances { get; set; } = new List<MeetingAttendance>();

        public virtual ICollection<MeetingInvitation> MeetingInvitations { get; set; } = new List<MeetingInvitation>();

        public int ConversationId { get; set; }

        public virtual Conversation? Conversation { get; set; }
    }
}
