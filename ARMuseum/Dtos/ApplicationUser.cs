using Microsoft.AspNetCore.Identity;

namespace ARMuseum.Dtos
{
    public class ApplicationUser : IdentityUser
    {
        public string? UFirstName { get; set; }
        public string? ULastName { get; set; }
        public DateTime UDateOfBirth { get; set; }
        public string? UPhone { get; set; }
        public string? UCountry { get; set; }
        public string? UImageName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? PasswordResetCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }
    }
}
