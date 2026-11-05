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
                LocationId = meeting.LocationId,
                LocationName = meeting.Location.Name,
                LocationAddress = meeting.Location.Address,
                MeetingDate = meeting.MeetingDate,
                CreatedAt = meeting.CreatedAt,

                UserIds = meeting.MeetingAttendances
                    .Select(ma => ma.UserId)
                    .ToList(),

                Users = meeting.MeetingAttendances
                    .Select(ma => UserMapper.ToResponse(ma.User))
                    .ToList()
            };
        }
    }
}