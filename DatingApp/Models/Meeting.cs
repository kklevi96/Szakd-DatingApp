using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class Meeting : BaseEntity
    {
        //[ForeignKey(nameof(Location))]
        [Required]
        public int LocationId { get; set; }

        [NotMapped]
        public virtual Location Location { get; set; }

        [NotMapped]
        public virtual ICollection<MeetingAttendance> MeetingAttendances { get; set; }
    }
}
