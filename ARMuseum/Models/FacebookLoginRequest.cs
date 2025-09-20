using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Models
{
    public class FacebookLoginRequest
    {
        [Required]
        public string AccessToken { get; set; }
    }
}