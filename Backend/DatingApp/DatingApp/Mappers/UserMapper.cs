using DatingApp.Dtos.Users;
using DatingApp.Models;

namespace DatingApp.Mappers
{
    public static class UserMapper
    {
        public static UserResponse ToResponse(DatingappUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                Age = CalculateAge(user.BirthDay),
                WantsRelationship = user.WantsRelationship,
                WantsFriendship = user.WantsFriendship,
                InterestedinWoman = user.InterestedinWoman,
                InterestedinMan = user.InterestedinMan,
                Gender = user.Gender,
                MBTICode = user.MBTICode
            };
        }

        private static int CalculateAge(DateTime birthDay)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDay.Year;

            if (birthDay.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
}