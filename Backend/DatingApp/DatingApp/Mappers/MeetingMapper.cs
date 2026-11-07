using DatingApp.Dtos.Meetings;
using DatingApp.Mappers;
using DatingApp.Models;

namespace DatingApp.Mappers
{
    public static class MeetingMapper
    {
        public static MeetingResponse ToResponse(Meeting meeting)
        {
            return new MeetingResponse
            {
                Id = meeting.Id,
                CreatedAt = meeting.CreatedAt,
                LocationId = meeting.LocationId,
                LocationName = meeting.Location.Name,
                LocationAddress = meeting.Location.Address,
                ConversationId = meeting.ConversationId,
                MeetingDate = meeting.MeetingDate,
                UserIds = meeting.MeetingAttendances
                    .Select(ma => ma.UserId)
                    .ToList(),
                Users = meeting.MeetingAttendances
                    .Select(ma => UserMapper.ToResponse(ma.User))
                    .ToList(),
            };
        }
    }
}