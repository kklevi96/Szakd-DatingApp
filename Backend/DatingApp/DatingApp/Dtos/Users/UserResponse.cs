using DatingApp.Dtos.Hobbies;

namespace DatingApp.Dtos.Users
{
    public class UserResponse
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public int Age { get; set; }
        public bool WantsRelationship { get; set; }
        public bool WantsFriendship { get; set; }
        public bool InterestedinWoman { get; set; }
        public bool InterestedinMan { get; set; }
        public string Gender { get; set; } = null!;
        public int MBTICode { get; set; }
        public List<HobbyResponse> Hobbies { get; set; } = new List<HobbyResponse>();
    }
}