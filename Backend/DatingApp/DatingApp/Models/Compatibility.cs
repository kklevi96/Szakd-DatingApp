using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Compatibility : BaseEntity
    {
        [Required]
        public string UserId_A { get; set; } = null!;

        [Required]
        public string UserId_B { get; set; } = null!;

        public virtual DatingappUser UserA { get; set; }
        public virtual DatingappUser UserB { get; set; }
    }
}
