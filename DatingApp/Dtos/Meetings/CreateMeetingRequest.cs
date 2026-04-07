namespace DatingApp.Dtos.Meetings
{
    public class CreateMeetingRequest
    {
        public int LocationId { get; set; }
        public List<string> UserIds { get; set; } = new();
    }
}