using System.ComponentModel.DataAnnotations;

namespace Task1Backend.Models
{
    public class LoginUserDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}