using System.ComponentModel.DataAnnotations.Schema;

namespace DatingApp.Models
{
    public class Location : BaseEntity
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; }

        public Location()
        {
            Meetings = new List<Meeting>();
        }
    }
}
