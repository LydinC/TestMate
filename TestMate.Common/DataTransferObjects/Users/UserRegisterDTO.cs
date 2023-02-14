using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.DataTransferObjects.Users
{
    public class UserRegisterDTO
    {

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z0-9]{8,}$", ErrorMessage = "Please enter a valid username.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$", ErrorMessage = "Password does not meet requirements")]
        [PasswordPropertyText]
        public string Password { get; set; } = null!;

    }
}
