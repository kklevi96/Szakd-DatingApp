using DatingApp.Dtos.Conversations;

namespace DatingApp.Services
{
    public interface IConversationService
    {
        Task<ConversationResponse> CreateConversationAsync(string currentUserId, CreateConversationRequest request);
        Task<List<ConversationResponse>> GetMyConversationsAsync(string currentUserId);
    }
}