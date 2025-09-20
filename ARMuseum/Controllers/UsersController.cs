using ARMuseum.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System;
using ARMuseum.Models;

namespace ARMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints in this controller require an authenticated user.
    public class UsersController : ControllerBase
    {
        private readonly OurDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // Constructor for dependency injection
        public UsersController(OurDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Users/my-tickets
        // Retrieves all active tickets for the currently logged-in user.
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString && !u.IsDeleted);
                if (tbUser == null)
                {
                    return Unauthorized("User record not found in database.");
                }

                var currentUtcTime = DateTime.UtcNow;

                // First, check for any tickets that should have expired by now and update their status.
                var ticketsToUpdate = await _context.TbBuyAtickets
                    .Where(b => b.UId == tbUser.UId && b.TSucces == "Yes" && b.IsExpiredExplicitly == false)
                    .Include(b => b.Ticket)
                    .ToListAsync();

                bool changed = false;
                foreach (var ticket in ticketsToUpdate)
                {
                    var expirationDateByTime = ticket.Ticket.TicketLimitHour > 0
                        ? ticket.TCreatedAt.AddHours(ticket.Ticket.TicketLimitHour)
                        : ticket.TCreatedAt.AddDays(3);

                    if (expirationDateByTime < currentUtcTime)
                    {
                        ticket.IsExpiredExplicitly = true;
                        changed = true;
                    }
                }
                if (changed) { await _context.SaveChangesAsync(); }

                // Now, retrieve the list of currently active tickets.
                var userTickets = await _context.TbBuyAtickets
                    .Where(b => b.UId == tbUser.UId && b.TSucces == "Yes" && b.IsExpiredExplicitly == false)
                    .Include(b => b.Ticket)
                    .Include(b => b.MIdNavigation)
                    .Select(b => new UserTicketDto
                    {
                        OrderId = b.TOrderId,
                        TicketId = b.TicketId,
                        MuseumId = b.MId,
                        MuseumName = b.MIdNavigation.MName,
                        // Construct the full image URL.
                        MuseumImageUrl = string.IsNullOrEmpty(b.MIdNavigation.MImageName) ? null :
                                     $"{Request.Scheme}://{Request.Host}/images/museums/{b.MIdNavigation.MImageName}",
                        TicketType = b.Ticket.TicketType,
                        TicketDescription = b.Ticket.TicketDescription,
                        Price = b.TAmountCents,
                        Currency = b.TCurrency,
                        PurchaseDate = b.TCreatedAt,
                        TicketLimitHours = b.Ticket.TicketLimitHour,
                        ExpirationDate = b.Ticket.TicketLimitHour > 0
                                     ? b.TCreatedAt.AddHours(b.Ticket.TicketLimitHour)
                                     : b.TCreatedAt.AddDays(3),
                        TimeLeft = (b.Ticket.TicketLimitHour > 0
                                  ? b.TCreatedAt.AddHours(b.Ticket.TicketLimitHour)
                                  : b.TCreatedAt.AddDays(3)) - currentUtcTime,
                        CurrentDurationMinutes = b.CurrentDurationMinutes,
                        IsExpiredExplicitly = b.IsExpiredExplicitly,
                        Status = "Active"
                    })
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToListAsync();

                if (!userTickets.Any())
                {
                    return NotFound("No active tickets found for this user.");
                }

                return Ok(userTickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMyTickets] Error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // GET: api/Users/my-expired-tickets
        // Retrieves all expired tickets for the currently logged-in user.
        [HttpGet("my-expired-tickets")]
        public async Task<IActionResult> GetMyExpiredTickets()
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString && !u.IsDeleted);
                if (tbUser == null)
                {
                    return Unauthorized("User record not found in database.");
                }
                var currentUtcTime = DateTime.UtcNow;
                var userTickets = await _context.TbBuyAtickets
                    .Where(b => b.UId == tbUser.UId && b.TSucces == "Yes" && b.IsExpiredExplicitly == true)
                    .Include(b => b.Ticket)
                    .Include(b => b.MIdNavigation)
                    .Select(b => new UserTicketDto
                    {
                        OrderId = b.TOrderId,
                        TicketId = b.TicketId,
                        MuseumId = b.MId,
                        MuseumName = b.MIdNavigation.MName,
                        // Construct the full image URL.
                        MuseumImageUrl = string.IsNullOrEmpty(b.MIdNavigation.MImageName) ? null :
                                     $"{Request.Scheme}://{Request.Host}/images/museums/{b.MIdNavigation.MImageName}",
                        TicketType = b.Ticket.TicketType,
                        TicketDescription = b.Ticket.TicketDescription,
                        Price = b.TAmountCents,
                        Currency = b.TCurrency,
                        PurchaseDate = b.TCreatedAt,
                        TicketLimitHours = b.Ticket.TicketLimitHour,
                        ExpirationDate = b.Ticket.TicketLimitHour > 0
                                     ? b.TCreatedAt.AddHours(b.Ticket.TicketLimitHour)
                                     : b.TCreatedAt.AddDays(3),
                        TimeLeft = (b.Ticket.TicketLimitHour > 0
                                  ? b.TCreatedAt.AddHours(b.Ticket.TicketLimitHour)
                                  : b.TCreatedAt.AddDays(3)) - currentUtcTime,
                        CurrentDurationMinutes = b.CurrentDurationMinutes,
                        IsExpiredExplicitly = b.IsExpiredExplicitly,
                        Status = "Expired"
                    })
                    .OrderByDescending(t => t.PurchaseDate)
                    .ToListAsync();

                if (!userTickets.Any())
                {
                    return NotFound("No explicitly expired tickets found for this user.");
                }
                return Ok(userTickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMyExpiredTickets] Error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}