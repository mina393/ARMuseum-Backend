using ARMuseum.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using ARMuseum.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;

namespace ARMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuseumsController : ControllerBase
    {
        private readonly OurDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Constructor for dependency injection
        public MuseumsController(OurDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Museums/GetMuseumsForHomePage
        // Retrieves a simplified list of museums for the home page display.
        [HttpGet("GetMuseumsForHomePage")]
        public async Task<IActionResult> GetMuseumsForHomePage()
        {
            try
            {
                var museums = await _context.TbMuseums
                    .Select(m => new MuseumForHomePageDto
                    {
                        Id = m.MId,
                        Name = m.MName,
                        // Construct the full image URL.
                        ImageUrl = string.IsNullOrEmpty(m.MImageName) ? null : $"{Request.Scheme}://{Request.Host}/images/museums/{m.MImageName}"
                    })
                    .ToListAsync();
                return Ok(museums);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMuseumsForHomePage: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Museums/GetMuseumDetails/{name}
        // Retrieves detailed information for a specific museum by its name.
        [HttpGet("GetMuseumDetails/{name}")]
        public async Task<IActionResult> GetMuseumDetails(string name)
        {
            try
            {
                var museumEntity = await _context.TbMuseums.FirstOrDefaultAsync(m => m.MName == name);

                if (museumEntity == null)
                {
                    return NotFound(new { message = "Museum not found." });
                }

                string addressablesBaseUrl = null;
                if (!string.IsNullOrEmpty(museumEntity.MMapName))
                {
                    string relativePath = museumEntity.MMapName.Replace('\\', '/');
                    if (!relativePath.StartsWith("/"))
                    {
                        relativePath = "/" + relativePath;
                    }
                    if (!relativePath.EndsWith("/"))
                    {
                        relativePath = relativePath + "/";
                    }
                    addressablesBaseUrl = $"{Request.Scheme}://{Request.Host}{relativePath}";
                }

                var museum = new MuseumDetailsDto
                {
                    Id = museumEntity.MId,
                    Name = museumEntity.MName,
                    Description = museumEntity.MHistory,
                    // Construct the full image URL.
                    ImageUrl = string.IsNullOrEmpty(museumEntity.MImageName) ? null : $"{Request.Scheme}://{Request.Host}/images/museums/{museumEntity.MImageName}",
                    AddressablesBaseUrl = addressablesBaseUrl
                };

                return Ok(museum);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMuseumDetails: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Museums/tickets
        // Retrieves all available ticket options with their current prices.
        [HttpGet("tickets")]
        public async Task<IActionResult> GetTicketOptions()
        {
            try
            {
                var ticketOptions = await _context.TbTickets
                    .Select(t => new TicketOptionDto
                    {
                        TicketId = t.TicketId,
                        TicketType = t.TicketType,
                        TicketDescription = t.TicketDescription,
                        Price = t.TbTicketPrices
                                 .OrderByDescending(tp => tp.TicketDate)
                                 .Select(tp => tp.TicketPrice)
                                 .FirstOrDefault(),
                        Currency = "EGP"
                    })
                    .ToListAsync();

                ticketOptions = ticketOptions.Where(to => to.Price > 0).ToList();

                if (!ticketOptions.Any())
                {
                    return NotFound("No ticket options found.");
                }

                return Ok(ticketOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTicketOptions: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Museums/launch/{museumId}/using-ticket/{orderId}
        // Authorizes and provides the launch URL for the virtual museum experience.
        [Authorize]
        [HttpGet("launch/{museumId}/using-ticket/{orderId}")]
        public async Task<IActionResult> LaunchVirtualMuseum(int museumId, int orderId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString);
            if (tbUser == null)
            {
                return Unauthorized(new { message = "User not found." });
            }

            var ticketPurchase = await _context.TbBuyAtickets
                .Include(b => b.Ticket)
                .FirstOrDefaultAsync(b => b.TOrderId == orderId && b.MId == museumId && b.UId == tbUser.UId);

            if (ticketPurchase == null)
            {
                return Unauthorized(new { message = "Ticket not found or you do not own this ticket." });
            }

            // Check if the ticket is valid (successful payment and not explicitly expired).
            bool isTicketValid = ticketPurchase.TSucces == "Yes" && !ticketPurchase.IsExpiredExplicitly;
            if (isTicketValid)
            {
                var expirationDate = ticketPurchase.Ticket.TicketLimitHour > 0
                    ? ticketPurchase.TCreatedAt.AddHours(ticketPurchase.Ticket.TicketLimitHour)
                    : ticketPurchase.TCreatedAt.AddDays(3);

                if (expirationDate < DateTime.UtcNow)
                {
                    isTicketValid = false;
                }
            }

            if (!isTicketValid)
            {
                return BadRequest(new { message = "This ticket is not active or has expired." });
            }

            var museum = await _context.TbMuseums.FindAsync(museumId);
            if (museum == null || string.IsNullOrEmpty(museum.MMapName))
            {
                return NotFound(new { message = "Museum path is not configured on the server." });
            }

            var webGlRelativePath = Path.Combine(museum.MMapName, "index.html").Replace('\\', '/');
            var fullUrl = $"{Request.Scheme}://{Request.Host}/{webGlRelativePath}";

            return Ok(new { webGlUrl = fullUrl });
        }
    }
}