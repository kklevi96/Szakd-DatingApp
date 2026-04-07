using DatingApp.Dtos.Auth;
using DatingApp.Dtos.Auth;
using System.Security.Claims;

namespace DatingApp.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(AuthRegisterRequest request);
        Task<AuthResponse> LoginAsync(AuthLoginRequest request);
        Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal);
    }
}