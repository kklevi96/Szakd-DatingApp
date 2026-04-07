using DatingApp.Data;
using DatingApp.Dtos.Messages;
using DatingApp.Dtos.Users;
using DatingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public MessageService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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

            return new MessageResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderUserId = message.SenderUserId,
                Content = message.Content,
                CreatedAt = message.CreatedAt,
                Sender = MapUser(sender)
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
                Sender = MapUser(m.Sender)
            }).ToList();
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