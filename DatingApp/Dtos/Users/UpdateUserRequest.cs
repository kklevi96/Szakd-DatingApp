namespace DatingApp.Dtos.Users
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime BirthDay { get; set; }
        public bool InterestedinWoman { get; set; }
        public bool InterestedinMan { get; set; }
        public string Gender { get; set; } = null!;
        public int MBTICode { get; set; }
    }
}