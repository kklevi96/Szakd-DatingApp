using DatingApp.Data;
using DatingApp.Dtos.Hobbies;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class HobbyService : IHobbyService
    {
        private readonly ApplicationDbContext _context;

        public HobbyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<HobbyResponse>> GetAllHobbiesAsync()
        {
            return await _context.Hobbies
                .OrderBy(h => h.Name)
                .Select(h => new HobbyResponse
                {
                    Id = h.Id,
                    Name = h.Name
                })
                .ToListAsync();
        }

        public async Task<List<HobbyResponse>> GetMyHobbiesAsync(string currentUserId)
        {
            return await _context.HobbyUsers
                .Where(hu => hu.UserId == currentUserId)
                .Select(hu => new HobbyResponse
                {
                    Id = hu.Hobby.Id,
                    Name = hu.Hobby.Name
                })
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public async Task<bool> AddHobbyAsync(string currentUserId, int hobbyId)
        {
            var hobbyExists = await _context.Hobbies.AnyAsync(h => h.Id == hobbyId);

            if (!hobbyExists)
            {
                return false;
            }

            var alreadyAdded = await _context.HobbyUsers
                .AnyAsync(hu => hu.UserId == currentUserId && hu.HobbyId == hobbyId);

            if (alreadyAdded)
            {
                return true;
            }

            _context.HobbyUsers.Add(new HobbyUser
            {
                UserId = currentUserId,
                HobbyId = hobbyId
            });

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RemoveHobbyAsync(string currentUserId, int hobbyId)
        {
            var hobbyUser = await _context.HobbyUsers
                .FirstOrDefaultAsync(hu => hu.UserId == currentUserId && hu.HobbyId == hobbyId);

            if (hobbyUser == null)
            {
                return false;
            }

            _context.HobbyUsers.Remove(hobbyUser);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}