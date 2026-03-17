namespace DatingApp.Models
{
    public class HobbyUser
    {
        public string UserId { get; set; }

        public int HobbyId { get; set; }

        public DatingappUser User { get; set; }

        public Hobby Hobby { get; set; }
    }
}
