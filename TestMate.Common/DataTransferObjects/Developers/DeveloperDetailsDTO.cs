using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace TestMate.Common.DataTransferObjects.Developers
{
    public class DeveloperDetailsDTO
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                           ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z0-9]+$",
                           ErrorMessage = "Please enter a valid username.")]
        public string Username { get; set; } = null!;

        [ReadOnly(true)]
        public string Password { get; set; } = null!;

        [ReadOnly(true)]
        public Boolean? IsActive { get; set; } = false;
    }
}
