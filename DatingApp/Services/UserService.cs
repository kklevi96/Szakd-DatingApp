using DatingApp.Data;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DatingApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<DatingappUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserService(UserManager<DatingappUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
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

        public async Task<List<DatingappUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }
    }
}