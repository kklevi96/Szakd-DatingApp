using DatingApp.Data;
using DatingApp.Dtos.Messages;
using DatingApp.Hubs;
using DatingApp.Mappers;
using DatingApp.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class MessageService : IMessageService
    {
        private readonly DatingAppDbContext _context;
        private readonly IUserService _userService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<MessageService> _logger;

        public MessageService(DatingAppDbContext context, IUserService userService, IHubContext<NotificationHub> hubContext, ILogger<MessageService> logger)
        {
            _context = context;
            _userService = userService;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<MessageResponse> SendMessageAsync(string currentUserId, SendMessageRequest request)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == request.ConversationId && cm.UserId == currentUserId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException("You are not a member of this conversation.");
            }

            var sender = await _userService.GetByIdAsync(currentUserId);

            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderUserId = currentUserId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await NotifyMessagesChangedAsync(message.ConversationId);

            return new MessageResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                Sender = UserMapper.ToResponse(sender)
            };
        }

        public async Task<List<MessageResponse>> GetMessagesByConversationAsync(string currentUserId, int conversationId)
        {
            var isMember = await _context.ConversationMembers
                .AnyAsync(cm => cm.ConversationId == conversationId && cm.UserId == currentUserId);

            if (!isMember)
            {
                throw new UnauthorizedAccessException("You are not a member of this conversation.");
            }

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderUserId = m.SenderUserId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                Sender = UserMapper.ToResponse(m.Sender)
            }).ToList();
        }

        private async Task NotifyMessagesChangedAsync(int conversationId)
        {
            try
            {
                var memberIds = await _context.ConversationMembers
                    .Where(cm => cm.ConversationId == conversationId)
                    .Select(cm => cm.UserId)
                    .ToListAsync();

                await _hubContext.Clients.Users(memberIds)
                    .SendAsync("MessagesChanged", conversationId);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Message saved, the notification sending error in conversation: {ConversationId}.",
                    conversationId);
            }
        }
    }
}