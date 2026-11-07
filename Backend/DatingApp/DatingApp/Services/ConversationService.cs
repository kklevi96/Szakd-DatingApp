using DatingApp.Data;
using DatingApp.Dtos.Conversations;
using DatingApp.Mappers;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class ConversationService : IConversationService
    {
        private readonly DatingAppDbContext _context;
        private readonly IUserService _userService;

        public ConversationService(DatingAppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<ConversationResponse> CreateConversationAsync(string currentUserId, CreateConversationRequest request)
        {
            if (request.OtherUserIds == null ||
                request.OtherUserIds.Count == 0 ||
                request.OtherUserIds.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Please select valid users.");
            }

            var otherUserIds = request.OtherUserIds
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (otherUserIds.Contains(currentUserId))
            {
                throw new ArgumentException(
                    "Do not include yourself in the selected users.");
            }

            if (!request.IsGroup && otherUserIds.Count != 1)
            {
                throw new ArgumentException(
                    "A direct conversation requires exactly one other user.");
            }
            else if (request.IsGroup && otherUserIds.Count < 2)
            {
                throw new ArgumentException(
                    "A group conversation requires at least two other users.");
            }

            var existingUserCount = await _context.Users
                .CountAsync(user => otherUserIds.Contains(user.Id));

            if (existingUserCount != otherUserIds.Count)
            {
                throw new KeyNotFoundException(
                    "One or more selected users could not be found.");
            }

            var conversationName = string.IsNullOrWhiteSpace(request.Name)
                ? null
                : request.Name.Trim();

            if (request.IsGroup &&
                conversationName != null &&
                conversationName.Length > 100)
            {
                throw new ArgumentException(
                    "The group name cannot exceed 100 characters.");
            }

            if (!request.IsGroup)
            {
                var otherUserId = otherUserIds[0];

                var existingConversation = await _context.Conversations
                    .Include(conversation => conversation.ConversationMembers)
                    .ThenInclude(member => member.User)
                    .FirstOrDefaultAsync(conversation =>
                        !conversation.IsGroup &&
                        conversation.ConversationMembers.Count == 2 &&
                        conversation.ConversationMembers.Any(member =>
                            member.UserId == currentUserId) &&
                        conversation.ConversationMembers.Any(member =>
                            member.UserId == otherUserId));

                if (existingConversation != null)
                {
                    return ConversationMapper.ToResponse(existingConversation);
                }

                var compatible = await _context.Compatibilities
                    .AnyAsync(compatibility =>
                        ((compatibility.UserId_A == currentUserId &&
                         compatibility.UserId_B == otherUserId) ||
                        (compatibility.UserId_A == otherUserId &&
                         compatibility.UserId_B == currentUserId)) &&
                        compatibility.UserA.WantsRelationship && compatibility.UserB.WantsRelationship &&
                        Math.Abs((long)compatibility.UserA.MBTICode - compatibility.UserB.MBTICode) <= 3 &&
                        ((compatibility.UserA.Gender == "male" && compatibility.UserB.InterestedinMan) ||
                         (compatibility.UserA.Gender == "female" && compatibility.UserB.InterestedinWoman)) &&
                        ((compatibility.UserB.Gender == "male" && compatibility.UserA.InterestedinMan) ||
                         (compatibility.UserB.Gender == "female" && compatibility.UserA.InterestedinWoman)));

                if (!compatible)
                {
                    throw new ArgumentException(
                        "You can only create a direct conversation with a compatible user.");
                }

            }

            var memberUserIds = otherUserIds
                .Append(currentUserId)
                .ToList();

            var conversation = new Conversation
            {
                IsGroup = request.IsGroup,
                Name = request.IsGroup ? conversationName : null,
                CreatedAt = DateTime.UtcNow,
                ConversationMembers = memberUserIds
                    .Select(userId => new ConversationMember
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList()
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var createdConversation = await _context.Conversations
                .Include(savedConversation => savedConversation.ConversationMembers)
                .ThenInclude(member => member.User)
                .FirstAsync(savedConversation =>
                    savedConversation.Id == conversation.Id);

            return ConversationMapper.ToResponse(createdConversation);
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

            return conversations.Select(ConversationMapper.ToResponse).ToList();
        }
    }
}