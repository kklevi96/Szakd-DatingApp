using DatingApp.Dtos.Users;

namespace DatingApp.Dtos.Compatibilities
{
    public class CompatibilityCheckResponse
    {
        public bool IsCompatible { get; set; }
        public string? Reason { get; set; }
        public CompatibilityResponse? Compatibility { get; set; }
        public UserResponse? OtherUser { get; set; }
    }
}