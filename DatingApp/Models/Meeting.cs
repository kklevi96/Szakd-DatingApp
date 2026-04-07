using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Meeting : BaseEntity
    {
        [Required]
        public int LocationId { get; set; }

        public DateTime MeetingDate { get; set; }

        public virtual Location Location { get; set; }

        public virtual ICollection<MeetingAttendance> MeetingAttendances { get; set; }
    }
}
