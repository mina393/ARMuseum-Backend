using ARMuseum.Dtos;
using ARMuseum.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using ARMuseum.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ARMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly OurDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _environment;
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string DateFormatForApi = "yyyy-MM-dd'T'HH:mm:ss";

        // Constructor for dependency injection
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment environment,
            OurDbContext context,
            JwtSettings jwtSettings,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _context = context;
            _jwtSettings = jwtSettings;
            _httpClientFactory = httpClientFactory;
        }

        // POST: api/Auth/Register
        // Handles new user registration.
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime dobForUserEntity = model.UDateOfBirth;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.UUserName,
                        Email = model.UEmail,
                        NormalizedEmail = _userManager.NormalizeEmail(model.UEmail),
                        NormalizedUserName = _userManager.NormalizeName(model.UUserName),
                        UFirstName = model.UFirstName,
                        ULastName = model.ULastName,
                        UDateOfBirth = dobForUserEntity,
                        UPhone = model.UPhone,
                        UCountry = model.UCountry,
                        IsDeleted = false
                    };
                    var result = await _userManager.CreateAsync(user, model.UPassword);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "USER");

                        // Handle profile image upload if provided
                        if (model.UImageFile != null && model.UImageFile.Length > 0)
                        {
                            try
                            {
                                var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "images", "users");
                                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                                var uniqueFileName = $"{user.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{Path.GetExtension(model.UImageFile.FileName)}";
                                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await model.UImageFile.CopyToAsync(fileStream); }

                                user.UImageName = uniqueFileName;
                                await _userManager.UpdateAsync(user);
                            }
                            catch (Exception imageEx) { ModelState.AddModelError("UImageFile", $"Image registration failed: {imageEx.Message}"); }
                        }

                        // Create a corresponding record in the TbUser table for additional details
                        var tbUser = new TbUser
                        {
                            AspNetUserId = user.Id,
                            UFirstName = user.UFirstName,
                            ULastName = user.ULastName,
                            UEmail = user.Email,
                            UPhone = user.UPhone,
                            UImageName = user.UImageName,
                            UUserName = user.UserName,
                            UDateOfBirth = user.UDateOfBirth,
                            UCountry = user.UCountry,
                            IsDeleted = false
                        };
                        _context.TbUsers.Add(tbUser);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var claims = BuildClaims(user, await _userManager.GetRolesAsync(user));
                        var tokenString = GenerateJwtToken(claims);

                        return Ok(new { Token = tokenString, Message = "User created successfully.", UserId = user.Id });
                    }
                    else
                    {
                        foreach (var error in result.Errors) { ModelState.AddModelError(string.Empty, error.Description); }
                        await transaction.RollbackAsync();
                        return BadRequest(ModelState);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        // POST: api/Auth/Login
        // Handles user login with username and password.
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.UUserName);
            if (user == null) { ModelState.AddModelError(string.Empty, "Invalid Login Attempt."); return Unauthorized(ModelState); }

            if (user.IsDeleted)
            {
                ModelState.AddModelError(string.Empty, "Account is deleted or inactive.");
                return Unauthorized(ModelState);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.UPassword, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var claims = BuildClaims(user, await _userManager.GetRolesAsync(user));
                var tokenString = GenerateJwtToken(claims);
                return Ok(new { Token = tokenString, Message = "Login successful.", UserId = user.Id });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt.");
                return Unauthorized(ModelState);
            }
        }

        // GET: api/Auth/Profile
        // Retrieves the profile of the currently authenticated user.
        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token or user ID not found in claims.");
            }

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null || appUser.IsDeleted)
            {
                return NotFound($"Application user not found for ID: {userId}");
            }

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(t => t.AspNetUserId == userId && !t.IsDeleted);

            DateTime effectiveDateOfBirth = tbUser?.UDateOfBirth ?? appUser.UDateOfBirth;
            string dateOfBirthStringForDto = effectiveDateOfBirth.ToString(DateFormatForApi);

            var userProfileDto = new UserProfileDto
            {
                UserId = appUser.Id,
                TbUserId = tbUser?.UId,
                Username = tbUser?.UUserName ?? appUser.UserName,
                UFirstName = tbUser?.UFirstName ?? appUser.UFirstName,
                ULastName = tbUser?.ULastName ?? appUser.ULastName,
                UEmail = tbUser?.UEmail ?? appUser.Email,
                UPhone = tbUser?.UPhone ?? appUser.PhoneNumber,
                UCountry = tbUser?.UCountry ?? appUser.UCountry,
                UDateOfBirth = dateOfBirthStringForDto,
                UImageName = tbUser?.UImageName ?? appUser.UImageName,
            };

            return Ok(userProfileDto);
        }

        // PUT: api/Auth/Profile
        // Updates the profile of the currently authenticated user.
        [Authorize]
        [HttpPut("Profile")]
        public async Task<IActionResult> UpdateUserProfile([FromForm] UpdateProfileRequestDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Invalid token or user ID not found in claims.");

            var appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return NotFound($"User not found for ID: {userId}");

            appUser.UFirstName = model.UFirstName;
            appUser.ULastName = model.ULastName;

            if (!string.Equals(appUser.Email, model.UEmail, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(appUser, model.UEmail);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors) ModelState.AddModelError(nameof(model.UEmail), error.Description);
                    return BadRequest(ModelState);
                }
            }

            if (!string.IsNullOrEmpty(model.UDateOfBirth))
            {
                if (DateTime.TryParseExact(model.UDateOfBirth, DateFormatForApi, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime parsedDate))
                {
                    appUser.UDateOfBirth = parsedDate;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.UDateOfBirth), $"Invalid date format. Expected: '{DateFormatForApi}'");
                    return BadRequest(ModelState);
                }
            }

            // Handle password change if old and new passwords are provided
            if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(appUser, model.OldPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors) { ModelState.AddModelError("PasswordChange", error.Description); }
                    return BadRequest(ModelState);
                }
            }

            // Handle image update if a new image file is provided
            if (model.UImageFile != null && model.UImageFile.Length > 0)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "images", "users");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Delete the old image if it exists
                    if (!string.IsNullOrEmpty(appUser.UImageName))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, appUser.UImageName);
                        if (System.IO.File.Exists(oldFilePath)) { System.IO.File.Delete(oldFilePath); }
                    }

                    var uniqueFileName = $"{appUser.Id}_{Guid.NewGuid().ToString("N").Substring(0, 8)}{Path.GetExtension(model.UImageFile.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.UImageFile.CopyToAsync(fileStream);
                    }
                    appUser.UImageName = uniqueFileName;
                }
                catch (Exception imageEx)
                {
                    ModelState.AddModelError("UImageFile", $"Failed to upload image: {imageEx.Message}");
                }
            }

            var updateAppUserResult = await _userManager.UpdateAsync(appUser);
            if (!updateAppUserResult.Succeeded)
            {
                foreach (var error in updateAppUserResult.Errors) { ModelState.AddModelError("ProfileUpdate", error.Description); }
                return BadRequest(ModelState);
            }

            // Also update the corresponding TbUser record
            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(t => t.AspNetUserId == userId);
            if (tbUser != null)
            {
                tbUser.UFirstName = appUser.UFirstName;
                tbUser.ULastName = appUser.ULastName;
                tbUser.UEmail = appUser.Email;
                tbUser.UDateOfBirth = appUser.UDateOfBirth;
                tbUser.UImageName = appUser.UImageName;
                tbUser.UUserName = appUser.UserName;
                _context.TbUsers.Update(tbUser);
                await _context.SaveChangesAsync();
            }

            return Ok(new UpdateProfileResponseDto { Message = "Profile updated successfully." });
        }

        // DELETE: api/Auth/DeleteAccount
        // Marks the current user's account as deleted (soft delete).
        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new DeleteAccountResponseDto { Success = false, Message = "Invalid token or user ID not found." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var appUser = await _userManager.FindByIdAsync(userIdString);
                    if (appUser == null)
                    {
                        return NotFound(new DeleteAccountResponseDto { Success = false, Message = "User not found." });
                    }

                    var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString);

                    appUser.IsDeleted = true;
                    var updateAppUserResult = await _userManager.UpdateAsync(appUser);

                    if (!updateAppUserResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        var errors = string.Join(" ", updateAppUserResult.Errors.Select(e => e.Description));
                        return BadRequest(new DeleteAccountResponseDto { Success = false, Message = $"Failed to mark account as deleted: {errors}" });
                    }

                    if (tbUser != null)
                    {
                        tbUser.IsDeleted = true;
                        _context.TbUsers.Update(tbUser);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok(new DeleteAccountResponseDto { Success = true, Message = "Account marked as deleted successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, new DeleteAccountResponseDto { Success = false, Message = $"Internal Server Error: {ex.Message}" });
                }
            }
        }

        // --- Facebook Login ---

        [HttpPost("FacebookLogin")]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Step 1: Validate the access token with Facebook.
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/me?fields=id,email,first_name,last_name&access_token={request.AccessToken}");

            if (!response.IsSuccessStatusCode)
            {
                return Unauthorized(new { Message = "Invalid Facebook token." });
            }

            var content = await response.Content.ReadAsStringAsync();
            var facebookUser = JsonSerializer.Deserialize<FacebookUserData>(content);

            if (facebookUser == null || string.IsNullOrEmpty(facebookUser.Id))
            {
                return Unauthorized(new { Message = "Failed to deserialize Facebook user data." });
            }

            // Step 2: Check if the user already exists.
            var user = await _userManager.FindByLoginAsync("Facebook", facebookUser.Id);

            if (user == null)
            {
                // Step 2a: If user not found by login, check if a user with the same email already exists.
                if (!string.IsNullOrEmpty(facebookUser.Email))
                {
                    user = await _userManager.FindByEmailAsync(facebookUser.Email);
                    if (user != null)
                    {
                        // If found, link the Facebook account to the existing user.
                        var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo("Facebook", facebookUser.Id, "Facebook"));
                        if (!addLoginResult.Succeeded)
                        {
                            return BadRequest(new { Message = "Failed to link Facebook account.", Errors = addLoginResult.Errors });
                        }
                    }
                }

                // Step 2b: If no user is found at all, create a new one.
                if (user == null)
                {
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var newUser = new ApplicationUser
                            {
                                UFirstName = facebookUser.FirstName,
                                ULastName = facebookUser.LastName,
                                Email = facebookUser.Email,
                                UserName = facebookUser.Email ?? $"fb_{facebookUser.Id}", // Use email or a unique username
                                EmailConfirmed = true,
                                IsDeleted = false
                            };

                            var createResult = await _userManager.CreateAsync(newUser);
                            if (!createResult.Succeeded)
                            {
                                await transaction.RollbackAsync();
                                return BadRequest(new { Message = "Failed to create user.", Errors = createResult.Errors });
                            }

                            await _userManager.AddToRoleAsync(newUser, "USER");
                            await _userManager.AddLoginAsync(newUser, new UserLoginInfo("Facebook", facebookUser.Id, "Facebook"));

                            // Create the corresponding TbUser record.
                            var tbUser = new TbUser
                            {
                                AspNetUserId = newUser.Id,
                                UFirstName = newUser.UFirstName,
                                ULastName = newUser.ULastName,
                                UEmail = newUser.Email,
                                UUserName = newUser.UserName,
                                IsDeleted = false
                            };
                            _context.TbUsers.Add(tbUser);
                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();
                            user = newUser; // Use the new user for token generation.
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return StatusCode(500, $"Internal server error: {ex.Message}");
                        }
                    }
                }
            }

            if (user.IsDeleted)
            {
                return Unauthorized(new { Message = "This account has been deactivated." });
            }

            // Step 3: Generate and return a JWT for the user.
            var roles = await _userManager.GetRolesAsync(user);
            var claims = BuildClaims(user, roles);
            var tokenString = GenerateJwtToken(claims);

            return Ok(new
            {
                Token = tokenString,
                Message = "Login successful.",
                UserId = user.Id
            });
        }

        // Helper class for deserializing the user data from Facebook's Graph API response.
        private class FacebookUserData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("email")]
            public string Email { get; set; }
            [JsonPropertyName("first_name")]
            public string FirstName { get; set; }
            [JsonPropertyName("last_name")]
            public string LastName { get; set; }
        }

        // --- Helper Methods ---

        private List<Claim> BuildClaims(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            };
            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }

            if (user.UDateOfBirth != default(DateTime))
            {
                claims.Add(new Claim("birthdate", user.UDateOfBirth.ToString(DateFormatForApi)));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }

        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}