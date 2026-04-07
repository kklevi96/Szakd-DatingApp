using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.Models
{
    public class MeetingAttendance : BaseEntity
    {
        [Required]
        public int MeetingId { get; set; }

        [Required]
        public string UserId { get; set; } = null!;

        public virtual Meeting Meeting { get; set; }

        public virtual DatingappUser User { get; set; }
    }
}
