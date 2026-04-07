using DatingApp.Data;
using DatingApp.Dtos.Compatibilities;
using DatingApp.Dtos.Users;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DatingApp.Services
{
    public class CompatibilityService : ICompatibilityService
    {
        //prompt küldése/generálása chatgpt-nek, érdemes ezt json-ben kérni tőle vissza
        //token feltöltése
        //backend hívja a chatgpt api-t

        //vagy algoritmikus megoldás, vagy akár egymást is erősíthetik/validálhatják

        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public CompatibilityService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<CompatibilityCheckResponse> CheckOrCreateCompatibilityAsync(string currentUserId, string otherUserId)
        {
            if (currentUserId == otherUserId)
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = false,
                    Reason = "You can't be compatible with yourself."
                };
            }

            var currentUser = await _userService.GetByIdAsync(currentUserId);
            var otherUser = await _userService.GetByIdAsync(otherUserId);

            var (userAId, userBId) = NormalizeUserPair(currentUserId, otherUserId);

            var existingCompatibility = await _context.Compatibilities
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .FirstOrDefaultAsync(c => c.UserId_A == userAId && c.UserId_B == userBId);

            if (existingCompatibility != null)
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = true,
                    Reason = "Compatibility already exists.",
                    Compatibility = MapCompatibility(existingCompatibility),
                    OtherUser = MapUser(otherUser)
                };
            }

            if (currentUser.MBTICode != otherUser.MBTICode)
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = false,
                    Reason = "MBTI codes don't match.",
                    OtherUser = MapUser(otherUser)
                };
            }

            var compatibility = new Compatibility
            {
                UserId_A = userAId,
                UserId_B = userBId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Compatibilities.Add(compatibility);
            await _context.SaveChangesAsync();

            var createdCompatibility = await _context.Compatibilities
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .FirstAsync(c => c.Id == compatibility.Id);

            return new CompatibilityCheckResponse
            {
                IsCompatible = true,
                Reason = "Compatibility created.",
                Compatibility = MapCompatibility(createdCompatibility),
                OtherUser = MapUser(otherUser)
            };
        }

        public async Task<List<CompatibilityResponse>> GetMyCompatibilitiesAsync(string currentUserId)
        {
            var compatibilities = await _context.Compatibilities
                .Include(c => c.UserA)
                .Include(c => c.UserB)
                .Where(c => c.UserId_A == currentUserId || c.UserId_B == currentUserId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return compatibilities.Select(MapCompatibility).ToList();
        }

        public async Task<int> GenerateMyCompatibilitiesAsync(string currentUserId)
        {
            var currentUser = await _userService.GetByIdAsync(currentUserId);

            var otherUsers = await _context.Users
                .Where(u => u.Id != currentUserId)
                .ToListAsync();

            var existingPairs = await _context.Compatibilities
                .Where(c => c.UserId_A == currentUserId || c.UserId_B == currentUserId)
                .Select(c => new { c.UserId_A, c.UserId_B })
                .ToListAsync();

            var existingSet = new HashSet<string>(
                existingPairs.Select(p => $"{p.UserId_A}|{p.UserId_B}")
            );

            var newCompatibilities = new List<Compatibility>();

            foreach (var otherUser in otherUsers)
            {
                if (currentUser.MBTICode != otherUser.MBTICode)
                {
                    continue;
                }

                var (userAId, userBId) = NormalizeUserPair(currentUser.Id, otherUser.Id);
                var pairKey = $"{userAId}|{userBId}";

                if (existingSet.Contains(pairKey))
                {
                    continue;
                }

                newCompatibilities.Add(new Compatibility
                {
                    UserId_A = userAId,
                    UserId_B = userBId,
                    CreatedAt = DateTime.UtcNow
                });

                existingSet.Add(pairKey);
            }

            if (newCompatibilities.Count > 0)
            {
                _context.Compatibilities.AddRange(newCompatibilities);
                await _context.SaveChangesAsync();
            }

            return newCompatibilities.Count;
        }

        private static (string userAId, string userBId) NormalizeUserPair(string userId1, string userId2)
        {
            return string.Compare(userId1, userId2, StringComparison.Ordinal) < 0
                ? (userId1, userId2)
                : (userId2, userId1);
        }

        private static CompatibilityResponse MapCompatibility(Compatibility compatibility)
        {
            return new CompatibilityResponse
            {
                Id = compatibility.Id,
                UserId_A = compatibility.UserId_A,
                UserId_B = compatibility.UserId_B,
                CreatedAt = compatibility.CreatedAt,
                UserA = compatibility.UserA == null ? null : MapUser(compatibility.UserA),
                UserB = compatibility.UserB == null ? null : MapUser(compatibility.UserB)
            };
        }

        private static UserResponse MapUser(DatingappUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                InterestedinWoman = user.InterestedinWoman,
                InterestedinMan = user.InterestedinMan,
                Gender = user.Gender,
                MBTICode = user.MBTICode
            };
        }
    }
}