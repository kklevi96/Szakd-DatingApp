using DatingApp.Data;
using DatingApp.Dtos.Conversations;
using DatingApp.Hubs;
using DatingApp.Mappers;
using Microsoft.AspNetCore.SignalR;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class ConversationService : IConversationService
    {
        private readonly DatingAppDbContext _context;
        private readonly IUserService _userService;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(DatingAppDbContext context, IUserService userService,
            IHubContext<NotificationHub> hubContext, ILogger<ConversationService> logger)
        {
            _context = context;
            _userService = userService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ConversationResponse> CreateConversationAsync(string currentUserId, CreateConversationRequest request)
        {
            if (request.OtherUserIds == null || request.OtherUserIds.Count == 0 ||
                request.OtherUserIds.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Please select valid users.");

            var otherUserIds = request.OtherUserIds.Distinct(StringComparer.Ordinal).ToList();

            if (otherUserIds.Contains(currentUserId))
                throw new ArgumentException("Do not include yourself in the selected users.");

            if (!request.IsGroup && otherUserIds.Count != 1)
                throw new ArgumentException("A direct conversation requires exactly one other user.");

            if (request.IsGroup && otherUserIds.Count < 2)
                throw new ArgumentException("A group conversation requires at least two other users.");

            var otherUsers = await _context.Users
                .Where(user => otherUserIds.Contains(user.Id))
                .ToListAsync();

            if (otherUsers.Count != otherUserIds.Count)
                throw new KeyNotFoundException("One or more selected users could not be found.");

            var conversationName = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim();

            if (request.IsGroup && conversationName != null && conversationName.Length > 100)
                throw new ArgumentException("The group name cannot exceed 100 characters.");

            var memberUserIds = otherUserIds.Append(currentUserId).ToList();
            var existingConversation = await ConversationQuery()
                .Where(conversation => conversation.IsGroup == request.IsGroup &&
                    conversation.ConversationMembers.Count == memberUserIds.Count &&
                    conversation.ConversationMembers.All(member => memberUserIds.Contains(member.UserId)))
                .OrderBy(conversation => conversation.CreatedAt)
                .ThenBy(conversation => conversation.Id)
                .FirstOrDefaultAsync();

            if (existingConversation != null)
                return ConversationMapper.ToResponse(existingConversation);

            var currentUser = await _userService.GetByIdAsync(currentUserId);
            var allWantFriendship = currentUser.WantsFriendship && otherUsers.All(user => user.WantsFriendship);

            if (request.IsGroup && !allWantFriendship)
                throw new ArgumentException("All group members must be looking for friendship.");

            if (!request.IsGroup && !allWantFriendship &&
                !await AreDatingCompatibleAsync(currentUserId, otherUserIds[0]))
                throw new ArgumentException("You can only create a direct conversation with a compatible user.");

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

            var createdConversation = await ConversationQuery().FirstAsync(c => c.Id == conversation.Id);

            if (_context.Database.CurrentTransaction == null)
                await NotifyConversationsChangedAsync(conversation.Id);

            return ConversationMapper.ToResponse(createdConversation);
        }

        public async Task<ConversationResponse> RenameConversationAsync(
            string currentUserId, int conversationId, RenameConversationRequest request)
        {
            var conversation = await ConversationQuery().FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new KeyNotFoundException("The conversation could not be found.");

            if (!conversation.ConversationMembers.Any(member => member.UserId == currentUserId))
                throw new UnauthorizedAccessException("You are not a member of this conversation.");

            if (!conversation.IsGroup)
                throw new ArgumentException("Only group conversations can be renamed.");

            var name = request.Name?.Trim();

            if (string.IsNullOrEmpty(name) || name.Length > 100)
                throw new ArgumentException("The group name must contain between 1 and 100 characters.");

            conversation.Name = name;
            await _context.SaveChangesAsync();

            try
            {
                var memberIds = conversation.ConversationMembers.Select(member => member.UserId).ToList();
                await _hubContext.Clients.Users(memberIds)
                    .SendAsync("ConversationRenamed", new { id = conversation.Id, name = conversation.Name });
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception,
                    "Group name saved, but the notification could not be sent: {ConversationId}.", conversationId);
            }

            return ConversationMapper.ToResponse(conversation);
        }

        public async Task NotifyConversationsChangedAsync(int conversationId)
        {
            try
            {
                var memberIds = await _context.ConversationMembers
                    .Where(member => member.ConversationId == conversationId)
                    .Select(member => member.UserId)
                    .ToListAsync();

                await _hubContext.Clients.Users(memberIds).SendAsync("ConversationsChanged");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception,
                    "Conversation saved, but the list notification could not be sent: {ConversationId}.", conversationId);
            }
        }

        private IQueryable<Conversation> ConversationQuery()
        {
            return _context.Conversations
                .Include(conversation => conversation.ConversationMembers)
                .ThenInclude(member => member.User)
                .ThenInclude(user => user.HobbyUsers)
                .ThenInclude(hu => hu.Hobby);
        }

        private Task<bool> AreDatingCompatibleAsync(string currentUserId, string otherUserId)
        {
            return _context.Compatibilities.AnyAsync(compatibility =>
                ((compatibility.UserId_A == currentUserId && compatibility.UserId_B == otherUserId) ||
                 (compatibility.UserId_A == otherUserId && compatibility.UserId_B == currentUserId)) &&
                compatibility.UserA.WantsRelationship && compatibility.UserB.WantsRelationship &&
                Math.Abs((long)compatibility.UserA.MBTICode - compatibility.UserB.MBTICode) <= 3 &&
                ((compatibility.UserA.Gender == "male" && compatibility.UserB.InterestedinMan) ||
                 (compatibility.UserA.Gender == "female" && compatibility.UserB.InterestedinWoman)) &&
                ((compatibility.UserB.Gender == "male" && compatibility.UserA.InterestedinMan) ||
                 (compatibility.UserB.Gender == "female" && compatibility.UserA.InterestedinWoman)));
        }

        public async Task<List<ConversationResponse>> GetMyConversationsAsync(string currentUserId)
        {
            var conversationIds = await _context.ConversationMembers
                .Where(cm => cm.UserId == currentUserId)
                .Select(cm => cm.ConversationId)
                .ToListAsync();

            var conversations = await _context.Conversations
                .AsSplitQuery()
                .Where(c => conversationIds.Contains(c.Id))
                .Include(c => c.ConversationMembers)
                .ThenInclude(cm => cm.User)
                .ThenInclude(user => user.HobbyUsers)
                .ThenInclude(hu => hu.Hobby)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return conversations.Select(ConversationMapper.ToResponse).ToList();
        }
    }
}