namespace DatingApp.Dtos.Auth
{
    public class AuthLoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}