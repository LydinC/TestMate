using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.DataTransferObjects.Users
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [PasswordPropertyText]
        public string Password { get; set; } = null!;
    }
}
