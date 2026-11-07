using DatingApp.Dtos.Users;
using Microsoft.AspNetCore.Identity;
using DatingApp.Models;
using System.Security.Claims;

namespace DatingApp.Services
{
    public interface IUserService
    {
        Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal);

        Task<IdentityResult?> UpdateSearchPreferencesAsync(
            ClaimsPrincipal principal, SearchPreferencesRequest request);

        string GetCurrentUserId(ClaimsPrincipal principal);
        Task<DatingappUser> GetByIdAsync(string userId);
        Task<List<DatingappUser>> GetAllAsync();
    }
}