// File: Controllers/PasswordResetController.cs
using ARMuseum.Data.Models;
using ARMuseum.Dtos;
using ARMuseum.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ARMuseum.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        // Constructor for dependency injection
        public PasswordResetController(IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _emailService = emailService;
            _userManager = userManager;
        }

        // Endpoint 1: Request a password reset code.
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            // Security Note: To prevent email enumeration attacks, always return a generic success message
            // regardless of whether the user was found.
            if (user != null && !user.IsDeleted)
            {
                // 1. Generate a random 6-digit activation code.
                var activationCode = new Random().Next(100000, 999999).ToString();

                // 2. Store the code and its expiration date in the user's account.
                user.PasswordResetCode = activationCode;
                user.ResetCodeExpiry = DateTime.UtcNow.AddMinutes(10); // Code is valid for 10 minutes.
                await _userManager.UpdateAsync(user);

                // 3. Send the email.
                var subject = "Your Password Reset Code";
                var body = $"Hello, <br/> Your activation code is: <strong>{activationCode}</strong>. <br/> This code will expire in 10 minutes.";
                await _emailService.SendPasswordResetEmailAsync(request.Email, subject, body);
            }

            return Ok(new { message = "If a matching account was found, an email has been sent to reset your password." });
        }

        // Endpoint 2: Reset the password using the activation code.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid email or activation code." });
            }

            // Validate the code and its expiration date.
            if (user.PasswordResetCode != request.ActivationCode || user.ResetCodeExpiry <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired activation code." });
            }

            // If the code is valid, generate the Identity-specific password reset token.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Reset the password using the generated token.
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                // Clear the reset code after successful use.
                user.PasswordResetCode = null;
                user.ResetCodeExpiry = null;
                await _userManager.UpdateAsync(user);

                return Ok(new { message = "Password has been reset successfully." });
            }

            // If it fails, return the errors.
            return BadRequest(new { message = "Failed to reset password.", errors = result.Errors });
        }
    }

    // --- DTOs for Password Reset ---

    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ActivationCode { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}