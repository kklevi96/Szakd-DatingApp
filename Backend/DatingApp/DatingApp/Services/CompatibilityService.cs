using DatingApp.Data;
using DatingApp.Dtos.Compatibilities;
using DatingApp.Dtos.Users;
using DatingApp.Mappers;
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

        private readonly DatingAppDbContext _context;
        private readonly IUserService _userService;

        public CompatibilityService(DatingAppDbContext context, IUserService userService)
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

            if (!currentUser.WantsRelationship || !otherUser.WantsRelationship)
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = false,
                    Reason = "Both users must be looking for a relationship.",
                    OtherUser = UserMapper.ToResponse(otherUser)
                };
            }

            if (!AreMutuallyInterested(currentUser, otherUser))
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = false,
                    Reason = "Gender preferences are not mutual.",
                    OtherUser = UserMapper.ToResponse(otherUser)
                };
            }

            if (Math.Abs((long)currentUser.MBTICode - otherUser.MBTICode) > 3)
            {
                return new CompatibilityCheckResponse
                {
                    IsCompatible = false,
                    Reason = "MBTI code difference exceeds 3.",
                    OtherUser = UserMapper.ToResponse(otherUser)
                };
            }

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
                    Compatibility = CompatibilityMapper.ToResponse(existingCompatibility),
                    OtherUser = UserMapper.ToResponse(otherUser)
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
                Compatibility = CompatibilityMapper.ToResponse(createdCompatibility),
                OtherUser = UserMapper.ToResponse(otherUser)
            };
        }

        public async Task<List<CompatibilityResponse>> GetMyCompatibilitiesAsync(string currentUserId)
        {
            var compatibilities = await _context.Compatibilities
                .AsSplitQuery()
                .Include(c => c.UserA).ThenInclude(user => user.HobbyUsers).ThenInclude(hu => hu.Hobby)
                .Include(c => c.UserB).ThenInclude(user => user.HobbyUsers).ThenInclude(hu => hu.Hobby)
                .Where(c => c.UserId_A == currentUserId || c.UserId_B == currentUserId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return compatibilities
                .Where(c => c.UserA.WantsRelationship && c.UserB.WantsRelationship &&
                            Math.Abs((long)c.UserA.MBTICode - c.UserB.MBTICode) <= 3 &&
                            AreMutuallyInterested(c.UserA, c.UserB))
                .Select(CompatibilityMapper.ToResponse)
                .ToList();
        }

        public async Task<int> GenerateMyCompatibilitiesAsync(string currentUserId)
        {
            var currentUser = await _userService.GetByIdAsync(currentUserId);

            if (!currentUser.WantsRelationship)
                return 0;

            var otherUsers = await _context.Users
                .Where(u => u.Id != currentUserId && u.WantsRelationship)
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
                if (Math.Abs((long)currentUser.MBTICode - otherUser.MBTICode) > 3 ||
                    !AreMutuallyInterested(currentUser, otherUser))
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

        public async Task<List<UserResponse>> GetFriendshipUsersAsync(string currentUserId)
        {
            var currentUser = await _userService.GetByIdAsync(currentUserId);

            if (!currentUser.WantsFriendship)
                return new List<UserResponse>();

            var users = await _context.Users
                .Where(user => user.Id != currentUserId && user.WantsFriendship)
                .Include(user => user.HobbyUsers)
                .ThenInclude(hu => hu.Hobby)
                .OrderBy(user => user.FirstName)
                .ToListAsync();

            return users.Select(UserMapper.ToResponse).ToList();
        }

        private static bool AreMutuallyInterested(DatingappUser user, DatingappUser otherUser)
        {
            return IsInterestedIn(user, otherUser.Gender) &&
                   IsInterestedIn(otherUser, user.Gender);
        }

        private static bool IsInterestedIn(DatingappUser user, string gender)
        {
            return gender switch
            {
                "male" => user.InterestedinMan,
                "female" => user.InterestedinWoman,
                _ => false
            };
        }

        private static (string userAId, string userBId) NormalizeUserPair(string userId1, string userId2)
        {
            return string.Compare(userId1, userId2, StringComparison.Ordinal) < 0
                ? (userId1, userId2)
                : (userId2, userId1);
        }


    }
}