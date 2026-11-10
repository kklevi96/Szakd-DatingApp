using DatingApp.Dtos.Conversations;

namespace DatingApp.Services
{
    public interface IConversationService
    {
        Task<ConversationResponse> CreateConversationAsync(string currentUserId, CreateConversationRequest request);
        Task<List<ConversationResponse>> GetMyConversationsAsync(string currentUserId);
        Task<int> GetUnreadConversationCountAsync(string currentUserId);
        Task<bool> MarkAsReadAsync(string currentUserId, int conversationId, MarkConversationReadRequest request);
        Task NotifyConversationsChangedAsync(int conversationId);
        Task<ConversationResponse> RenameConversationAsync(string currentUserId, int conversationId, RenameConversationRequest request);
    }
}
