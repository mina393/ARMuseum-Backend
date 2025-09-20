using ARMuseum.Dtos;
using ARMuseum.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ARMuseum.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using ARMuseum.Models.ARMuseum.Dtos;

namespace ARMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly OurDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        // Constructor to inject dependencies
        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtSettings jwtSettings,
            OurDbContext context,
            IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // POST: api/Admin/Login
        // Handles admin user login.
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.UUserName);
            if (user == null || user.IsDeleted) return Unauthorized(new { Message = "Invalid username or password." });

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin) return Unauthorized(new { Message = "Access Denied. User is not an admin." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.UPassword, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = BuildClaims(user, roles);
                var tokenString = GenerateJwtToken(claims);
                return Ok(new { Token = tokenString, Message = "Admin login successful.", UserId = user.Id });
            }

            return Unauthorized(new { Message = "Invalid username or password." });
        }

        // --- User Management ---

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var identityUsers = await _userManager.Users.ToListAsync();
            var usersDto = new List<UserForAdminDto>();

            foreach (var identityUser in identityUsers)
            {
                var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == identityUser.Id);

                usersDto.Add(new UserForAdminDto
                {
                    Id = identityUser.Id,
                    UserName = identityUser.UserName,
                    Email = identityUser.Email,
                    FirstName = tbUser?.UFirstName ?? "N/A",
                    LastName = tbUser?.ULastName ?? "N/A",
                    PhoneNumber = tbUser?.UPhone,
                    IsDeleted = identityUser.IsDeleted,
                    Roles = await _userManager.GetRolesAsync(identityUser)
                });
            }
            return Ok(usersDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == id);

            var userDto = new UserForAdminDto
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName,
                Email = identityUser.Email,
                FirstName = tbUser?.UFirstName ?? "N/A",
                LastName = tbUser?.ULastName ?? "N/A",
                PhoneNumber = tbUser?.UPhone,
                IsDeleted = identityUser.IsDeleted,
                Roles = await _userManager.GetRolesAsync(identityUser)
            };
            return Ok(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("User/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserByAdminRequest model)
        {
            var identityUser = await _userManager.FindByIdAsync(id);
            if (identityUser == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            identityUser.IsDeleted = model.IsDeleted;
            if (identityUser.Email != model.Email)
            {
                await _userManager.SetEmailAsync(identityUser, model.Email);
                await _userManager.SetUserNameAsync(identityUser, model.Email);
            }
            await _userManager.UpdateAsync(identityUser);

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == id);
            if (tbUser != null)
            {
                tbUser.UFirstName = model.FirstName;
                tbUser.ULastName = model.LastName;
                tbUser.UPhone = model.PhoneNumber;
                _context.TbUsers.Update(tbUser);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "User updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("User/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound(new { Message = "User not found." });

            user.IsDeleted = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) return Ok(new { Message = "User successfully marked as deleted." });

            return BadRequest(result.Errors);
        }

        // --- Museum Management ---

        [HttpGet("Museums")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMuseums()
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var museums = await _context.TbMuseums
                .Select(m => new MuseumDto
                {
                    Id = m.MId,
                    Name = m.MName,
                    History = m.MHistory,
                    ImageName = m.MImageName,
                    ImageUrl = $"{baseUrl}/images/museums/{m.MImageName}"
                }).ToListAsync();
            return Ok(museums);
        }

        [HttpGet("Museums/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMuseumById(int id)
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var museum = await _context.TbMuseums
                .Where(m => m.MId == id)
                .Select(m => new MuseumDto
                {
                    Id = m.MId,
                    Name = m.MName,
                    History = m.MHistory,
                    ImageName = m.MImageName,
                    ImageUrl = $"{baseUrl}/images/museums/{m.MImageName}"
                }).FirstOrDefaultAsync();

            if (museum == null)
            {
                return NotFound();
            }
            return Ok(museum);
        }

        [HttpPost("Museums")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMuseum([FromForm] CreateOrUpdateMuseumDto model)
        {
            if (!ModelState.IsValid || model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return BadRequest("Invalid data or missing image file.");
            }

            var imageName = await SaveImage(model.ImageFile, "museums");
            var museum = new TbMuseum
            {
                MName = model.Name,
                MHistory = model.History,
                MImageName = imageName
            };
            _context.TbMuseums.Add(museum);
            await _context.SaveChangesAsync();
            return StatusCode(201, new { Message = "Museum created successfully." });
        }

        [HttpPut("Museums/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMuseum(int id, [FromForm] CreateOrUpdateMuseumDto model)
        {
            var museum = await _context.TbMuseums.FindAsync(id);
            if (museum == null)
            {
                return NotFound();
            }

            museum.MName = model.Name;
            museum.MHistory = model.History;

            if (model.ImageFile != null)
            {
                DeleteImage(museum.MImageName, "museums");
                museum.MImageName = await SaveImage(model.ImageFile, "museums");
            }

            _context.TbMuseums.Update(museum);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Museum updated successfully." });
        }

        [HttpDelete("Museums/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMuseum(int id)
        {
            var museum = await _context.TbMuseums.FindAsync(id);
            if (museum == null)
            {
                return NotFound();
            }

            DeleteImage(museum.MImageName, "museums");
            _context.TbMuseums.Remove(museum);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Museum deleted successfully." });
        }

        // --- Ticket Management ---

        [HttpGet("Tickets")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTicketsForAdmin()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tickets = await _context.TbTickets
                .Select(t => new TicketAdminDto
                {
                    TicketId = t.TicketId,
                    TicketType = t.TicketType,
                    TicketLimitHour = t.TicketLimitHour,
                    TicketDescription = t.TicketDescription,
                    CurrentPrice = t.TbTicketPrices
                                     .Where(p => p.TicketDate <= today)
                                     .OrderByDescending(p => p.TicketDate)
                                     .Select(p => p.TicketPrice)
                                     .FirstOrDefault()
                }).ToListAsync();
            return Ok(tickets);
        }

        [HttpPut("Tickets/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto model)
        {
            var ticket = await _context.TbTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            ticket.TicketLimitHour = model.TicketLimitHour;
            _context.TbTickets.Update(ticket);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var priceForToday = await _context.TbTicketPrices
                                            .FirstOrDefaultAsync(p => p.TicketId == id && p.TicketDate == today);

            if (priceForToday != null)
            {
                priceForToday.TicketPrice = model.NewPrice;
                _context.TbTicketPrices.Update(priceForToday);
            }
            else
            {
                var newPrice = new TbTicketPrice
                {
                    TicketId = id,
                    TicketDate = today,
                    TicketPrice = model.NewPrice
                };
                _context.TbTicketPrices.Add(newPrice);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Ticket updated successfully." });
        }

        // --- Helper Methods ---

        private List<Claim> BuildClaims(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

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

        // --- Helper Methods for Images ---

        private async Task<string> SaveImage(IFormFile imageFile, string subfolder)
        {
            string imageName = new String(Path.GetFileNameWithoutExtension(imageFile.FileName)
                                            .Take(10)
                                            .ToArray())
                                            .Replace(' ', '-');

            imageName = $"{imageName}{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(imageFile.FileName)}";
            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", subfolder, imageName);

            var directoryPath = Path.GetDirectoryName(imagePath);
            if (!Directory.Exists(directoryPath!))
            {
                Directory.CreateDirectory(directoryPath!);
            }

            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return imageName;
        }

        private void DeleteImage(string imageName, string subfolder)
        {
            if (string.IsNullOrEmpty(imageName)) return;

            var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "images", subfolder, imageName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}