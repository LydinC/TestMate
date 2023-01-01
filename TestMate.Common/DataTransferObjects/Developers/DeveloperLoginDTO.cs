using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.DataTransferObjects.Developers
{
    public class DeveloperLoginDTO
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }
}
