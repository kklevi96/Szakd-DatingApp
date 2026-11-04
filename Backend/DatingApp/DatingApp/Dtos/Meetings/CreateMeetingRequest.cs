namespace DatingApp.Dtos.Meetings
{
    public class CreateMeetingRequest
    {
        public int LocationId { get; set; }
        public DateTime MeetingDate { get; set; }
    }
}