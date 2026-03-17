using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Compatibility : BaseEntity
    {
        //[ForeignKey(nameof(DatingappUser))]
        [Required]
        public string UserId_A { get; set; } = null!;

        //[ForeignKey(nameof(DatingappUser))]
        [Required]
        public string UserId_B { get; set; } = null!;

        [NotMapped]
        public virtual DatingappUser UserA { get; set; }
        [NotMapped]
        public virtual DatingappUser UserB { get; set; }
    }
}
