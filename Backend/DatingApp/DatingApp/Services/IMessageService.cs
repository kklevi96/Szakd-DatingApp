using DatingApp.Dtos.Messages;

namespace DatingApp.Services
{
    public interface IMessageService
    {
        Task<MessageResponse> SendMessageAsync(string currentUserId, SendMessageRequest request);
        Task<List<MessageResponse>> GetMessagesByConversationAsync(string currentUserId, int conversationId);
    }
}