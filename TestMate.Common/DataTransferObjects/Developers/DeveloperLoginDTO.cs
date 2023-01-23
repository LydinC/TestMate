using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.DataTransferObjects.Developers
{
    public class DeveloperLoginDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [PasswordPropertyText]
        public string Password { get; set; } = null!;
    }
}
