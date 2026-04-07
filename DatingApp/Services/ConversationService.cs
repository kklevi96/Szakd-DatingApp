using DatingApp.Data;
using DatingApp.Dtos.Conversations;
using DatingApp.Dtos.Users;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public ConversationService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<ConversationResponse> CreateConversationAsync(string currentUserId, CreateConversationRequest request)
        {
            if (currentUserId == request.OtherUserId)
            {
                throw new Exception("You cannot create a conversation with yourself.");
            }

            var currentUser = await _userService.GetByIdAsync(currentUserId);
            var otherUser = await _userService.GetByIdAsync(request.OtherUserId);

            var compatible = await _context.Compatibilities.AnyAsync(c =>
                (c.UserId_A == currentUserId && c.UserId_B == request.OtherUserId) ||
                (c.UserId_A == request.OtherUserId && c.UserId_B == currentUserId));

            if (!compatible)
            {
                throw new Exception("You can only create a conversation with a compatible user.");
            }

            var currentUserConversationIds = await _context.ConversationMembers
                .Where(cm => cm.UserId == currentUserId)
                .Select(cm => cm.ConversationId)
                .ToListAsync();

            var existingConversationId = await _context.ConversationMembers
                .Where(cm => currentUserConversationIds.Contains(cm.ConversationId))
                .GroupBy(cm => cm.ConversationId)
                .Where(g => g.Count() == 2)
                .Where(g => g.Any(x => x.UserId == currentUserId) && g.Any(x => x.UserId == request.OtherUserId))
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            if (existingConversationId != 0)
            {
                var existingConversation = await _context.Conversations
                    .Include(c => c.ConversationMembers)
                        .ThenInclude(cm => cm.User)
                    .FirstAsync(c => c.Id == existingConversationId);

                return MapConversation(existingConversation);
            }

            var conversation = new Conversation
            {
                CreatedAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var members = new List<ConversationMember>
            {
                new ConversationMember
                {
                    ConversationId = conversation.Id,
                    UserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                },
                new ConversationMember
                {
                    ConversationId = conversation.Id,
                    UserId = request.OtherUserId,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _context.ConversationMembers.AddRange(members);
            await _context.SaveChangesAsync();

            var createdConversation = await _context.Conversations
                .Include(c => c.ConversationMembers)
                    .ThenInclude(cm => cm.User)
                .FirstAsync(c => c.Id == conversation.Id);

            return MapConversation(createdConversation);
        }

        public async Task<List<ConversationResponse>> GetMyConversationsAsync(string currentUserId)
        {
            var conversationIds = await _context.ConversationMembers
                .Where(cm => cm.UserId == currentUserId)
                .Select(cm => cm.ConversationId)
                .ToListAsync();

            var conversations = await _context.Conversations
                .Where(c => conversationIds.Contains(c.Id))
                .Include(c => c.ConversationMembers)
                .ThenInclude(cm => cm.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return conversations.Select(MapConversation).ToList();
        }

        private static ConversationResponse MapConversation(Conversation conversation)
        {
            return new ConversationResponse
            {
                Id = conversation.Id,
                CreatedAt = conversation.CreatedAt,
                MemberUserIds = conversation.ConversationMembers.Select(cm => cm.UserId).ToList(),
                Members = conversation.ConversationMembers.Select(cm => MapUser(cm.User)).ToList()
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