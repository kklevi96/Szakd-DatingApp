using DatingApp.Dtos.Compatibilities;

namespace DatingApp.Services
{
    public interface ICompatibilityService
    {
        Task<CompatibilityCheckResponse> CheckOrCreateCompatibilityAsync(string currentUserId, string otherUserId);
        Task<List<CompatibilityResponse>> GetMyCompatibilitiesAsync(string currentUserId);
        Task<int> GenerateMyCompatibilitiesAsync(string currentUserId);
    }
}