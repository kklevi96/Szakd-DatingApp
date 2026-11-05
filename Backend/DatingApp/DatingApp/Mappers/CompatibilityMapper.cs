using DatingApp.Dtos.Compatibilities;
using DatingApp.Models;

namespace DatingApp.Mappers
{
    public static class CompatibilityMapper
    {
        public static CompatibilityResponse ToResponse(Compatibility compatibility)
        {
            return new CompatibilityResponse
            {
                Id = compatibility.Id,
                UserId_A = compatibility.UserId_A,
                UserId_B = compatibility.UserId_B,
                CreatedAt = compatibility.CreatedAt,
                UserA = compatibility.UserA == null ? null : UserMapper.ToResponse(compatibility.UserA),
                UserB = compatibility.UserB == null ? null : UserMapper.ToResponse(compatibility.UserB)
            };
        }
    }
}
