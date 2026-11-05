namespace DatingApp.Models
{
    public class MeetingInvitation : BaseEntity
    {
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DatingappUser User { get; set; } = null!;
    }
}
