using DatingApp.Models;
using System.Security.Claims;

namespace DatingApp.Services
{
    public interface IUserService
    {
        string GetCurrentUserId(ClaimsPrincipal principal);
        Task<DatingappUser> GetByIdAsync(string userId);
        Task<List<DatingappUser>> GetAllAsync();
    }
}