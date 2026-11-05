using System.ComponentModel.DataAnnotations;

namespace DatingApp.Dtos.Auth
{
    public class DelegateAdminRequest
    {
        [Required]
        public string Password { get; set; } = null!;
    }
}
