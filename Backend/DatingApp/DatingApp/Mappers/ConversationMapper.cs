using DatingApp.Dtos.Conversations;
using DatingApp.Models;

namespace DatingApp.Mappers
{
    public static class ConversationMapper
    {
        public static ConversationResponse ToResponse(Conversation conversation)
        {
            return new ConversationResponse
            {
                Id = conversation.Id,
                IsGroup = conversation.IsGroup,
                Name = conversation.Name,
                CreatedAt = conversation.CreatedAt,
                MemberUserIds = conversation.ConversationMembers
                    .Select(member => member.UserId)
                    .ToList(),
                Members = conversation.ConversationMembers
                    .Select(member => UserMapper.ToResponse(member.User))
                    .ToList()
            };
        }
    }
}
