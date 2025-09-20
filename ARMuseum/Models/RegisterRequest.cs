using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace ARMuseum.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be between {2} and {1} characters.", MinimumLength = 5)]
        public string UUserName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [ValidAge(MinAge = 5, MaxAge = 100, ErrorMessage = "User must be between 5 and 100 years old.")]
        public DateTime UDateOfBirth { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name must be between {2} and {1} characters.", MinimumLength = 3)]
        public string UFirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "The last name must be between {2} and {1} characters.", MinimumLength = 3)]
        public string ULastName { get; set; }

        [Required(ErrorMessage = "Password required.")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters.", MinimumLength = 8)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z\\d]).{8,}$",
            ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string UPassword { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(100, ErrorMessage = "Invalid phone number.", MinimumLength = 11)]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string UPhone { get; set; }

        [Required(ErrorMessage = "The Country is required.")]
        [StringLength(500, ErrorMessage = "The country name is too long.")]
        public string UCountry { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(300, ErrorMessage = "The email is too long.")]
        public string UEmail { get; set; }

        public IFormFile? UImageFile { get; set; }
    }

    public class ValidAgeAttribute : ValidationAttribute
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                var today = DateTime.Today;
                var age = today.Year - dateOfBirth.Year;
                if (dateOfBirth.Date > today.AddYears(-age))
                {
                    age--; // Adjust age if birthday hasn't occurred this year
                }

                if (age < MinAge || age > MaxAge)
                {
                    // Using the ErrorMessage from the attribute directly or a formatted string
                    return new ValidationResult(ErrorMessage ?? $"Must be between {MinAge} and {MaxAge} years old.", new[] { validationContext.MemberName });
                }
            }
            return ValidationResult.Success;
        }
    }
}