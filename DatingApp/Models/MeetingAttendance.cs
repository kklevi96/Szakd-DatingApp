using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class MeetingAttendance : BaseEntity
    {
        //[ForeignKey(nameof(Meeting))]
        [Required]
        public int MeetingId { get; set; }

        //[ForeignKey(nameof(DatingappUser))]
        [Required]
        public string UserId { get; set; } = null!;

        [NotMapped]
        public virtual Meeting Meeting { get; set; }

        [NotMapped]
        public virtual DatingappUser User { get; set; }
    }
}
