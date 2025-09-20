using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be between {5} and {20} characters.", MinimumLength = 5)]
        public string UUserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string UPassword { get; set; }
    }
}
