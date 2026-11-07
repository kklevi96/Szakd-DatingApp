using DatingApp.Dtos.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DatingApp.Services
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(AuthRegisterRequest request);
        Task<AuthResponse?> LoginAsync(AuthLoginRequest request);
        Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<IdentityResult?> UpdateSearchPreferencesAsync(ClaimsPrincipal principal, SearchPreferencesRequest request);
        Task<IdentityResult?> DelegateAdminAsync(ClaimsPrincipal principal, DelegateAdminRequest request);
        Task<IdentityResult?> RemoveAdminAsync(ClaimsPrincipal principal);
    }
}
