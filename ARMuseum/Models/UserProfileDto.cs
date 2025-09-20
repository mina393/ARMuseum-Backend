using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Models
{
    // DTO for fetching and displaying user profile data.
    // Similar in structure to a login or registration response.
    public class UserProfileDto
    {
        public string UserId { get; set; }
        public int? TbUserId { get; set; }
        public string Username { get; set; }
        public string UFirstName { get; set; }
        public string ULastName { get; set; }
        public string UEmail { get; set; }
        public string UPhone { get; set; }
        public string UCountry { get; set; }
        public string UDateOfBirth { get; set; } // Using string for date of birth for flexible formatting in the API.
        public string? UImageName { get; set; }
    }

    // DTO for receiving data to update a user's profile.
    public class UpdateProfileRequestDto
    {
        // Fields that the user is allowed to edit from the UI.
        [Required] public string UFirstName { get; set; }
        [Required] public string ULastName { get; set; }
        [Required][EmailAddress] public string UEmail { get; set; }
        public string UDateOfBirth { get; set; }

        // Optional fields for changing the password.
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }

        // Optional field for uploading a new profile image.
        public IFormFile? UImageFile { get; set; }
    }

    // DTO for the response after a profile update.
    public class UpdateProfileResponseDto
    {
        public string Message { get; set; }
        // You can return the updated profile data here if needed.
        // public UserProfileDto UpdatedProfile { get; set; }
    }
}