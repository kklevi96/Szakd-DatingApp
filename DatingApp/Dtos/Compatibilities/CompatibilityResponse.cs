using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Compatibilities
{
    public class CompatibilityResponse
    {
        public int Id { get; set; }
        public string UserId_A { get; set; } = null!;
        public string UserId_B { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public UserResponse? UserA { get; set; }
        public UserResponse? UserB { get; set; }
    }
}