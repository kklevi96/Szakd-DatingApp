using DatingApp.Dtos.Hobbies;

namespace DatingApp.Services
{
    public interface IHobbyService
    {
        Task<List<HobbyResponse>> GetAllHobbiesAsync();
        Task<List<HobbyResponse>> GetMyHobbiesAsync(string currentUserId);
        Task<bool> AddHobbyAsync(string currentUserId, int hobbyId);
        Task<bool> RemoveHobbyAsync(string currentUserId, int hobbyId);
    }
}
