using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.Models.Developers
{
    public class Developer
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

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
        [RegularExpression(@"^[a-zA-Z0-9]{8,}$",
                           ErrorMessage = "Please enter a valid username.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$", ErrorMessage = "Password does not meet requirements")]
        public string Password { get; set; } = null!;

        [Required]
        public Boolean IsActive { get; set; }

    }
}
