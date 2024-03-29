﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.DataTransferObjects.Users
{
    public class UserDetailsDTO
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
        [RegularExpression(@"^[a-zA-Z0-9]+$",
                           ErrorMessage = "Please enter a valid username.")]
        public string Username { get; set; } = null!;

        [PasswordPropertyText]
        public string Password { get; set; } = null!;

        [ReadOnly(true)]
        public Boolean? IsActive { get; set; } = false;
    }
}
