using DatingApp.Dtos.Compatibilities;
using DatingApp.Dtos.Users;

namespace DatingApp.Services
{
    public interface ICompatibilityService
    {
        Task<CompatibilityCheckResponse> CheckOrCreateCompatibilityAsync(string currentUserId, string otherUserId);
        Task<List<CompatibilityResponse>> GetMyCompatibilitiesAsync(string currentUserId);
        Task<List<UserResponse>> GetFriendshipUsersAsync(string currentUserId);
        Task<int> GenerateMyCompatibilitiesAsync(string currentUserId);
    }
}