using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Hobby : BaseEntity
    {
        public string Name { get; set; } = null!;

        public virtual ICollection<HobbyUser> HobbyUsers { get; set; }
    }
}
