using DatingApp.Dtos.Users;
using DatingApp.Data;
using DatingApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DatingApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<DatingappUser> _userManager;
        private readonly DatingAppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<DatingappUser> userManager,
            DatingAppDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public string GetCurrentUserId(ClaimsPrincipal principal)
        {
            var userId = _userManager.GetUserId(principal);

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Not logged in.");
            }

            return userId;
        }

        public async Task<DatingappUser> GetByIdAsync(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new Exception("User cannot be found.");
            }

            return user;
        }

        public async Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);

            if (user == null)
                return null;

            return new UserMeResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                WantsRelationship = user.WantsRelationship,
                WantsFriendship = user.WantsFriendship,
                InterestedinWoman = user.InterestedinWoman,
                InterestedinMan = user.InterestedinMan,
                Gender = user.Gender,
                MBTICode = user.MBTICode,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                CanRequestAdmin = true
            };
        }

        public async Task<IdentityResult?> UpdateSearchPreferencesAsync(
            ClaimsPrincipal principal, SearchPreferencesRequest request)
        {
            var user = await _userManager.GetUserAsync(principal);
            if (user == null)
                return null;

            var relationshipChanged = user.WantsRelationship != request.WantsRelationship;
            var friendshipChanged = user.WantsFriendship != request.WantsFriendship;

            user.WantsRelationship = request.WantsRelationship;
            user.WantsFriendship = request.WantsFriendship;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded && relationshipChanged)
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("MatchesChanged");
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Match notification sending error, but changes saved");
                }
            }

            if (result.Succeeded && friendshipChanged)
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("PotentialFriendsChanged");
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "Friend notification sending error, but changes saved.");
                }
            }

            return result;
        }

        public async Task<List<DatingappUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}
