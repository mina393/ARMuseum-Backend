// File: ARMuseum/Hubs/MuseumTrackingHub.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ARMuseum.Data.Models;

namespace ARMuseum.Hubs
{
    [Authorize] // Ensure the Hub is protected
    public class MuseumTrackingHub : Hub
    {
        private readonly OurDbContext _context;

        public MuseumTrackingHub(OurDbContext context)
        {
            _context = context;
        }

        // This function can be called from the client (e.g., Android)
        // when the user enters the AR Experience for the museum.
        public async Task UserEnteredMuseum(int museumId, int ticketOrderId)
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                // This should not happen with [Authorize], but as a precaution.
                throw new HubException("User not authenticated.");
            }

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString && !u.IsDeleted);
            if (tbUser == null)
            {
                throw new HubException("User record not found.");
            }

            // Find the active ticket for the user and museum.
            var ticket = await _context.TbBuyAtickets
                .Include(t => t.Ticket) // to ensure ticket data is loaded
                .FirstOrDefaultAsync(b => b.TOrderId == ticketOrderId && b.UId == tbUser.UId && b.MId == museumId && b.TSucces == "Yes" && b.IsExpiredExplicitly == false);

            if (ticket != null)
            {
                // You can store the entry time here or update the duration directly if you track it another way.
                // Example: simply log an entry event.
                // Console.WriteLine($"User {userIdString} entered Museum {museumId} with Ticket {ticketOrderId}");

                // It's better to update the duration periodically from the client or using a server-side timer.
                // This is just an example: you can send a message to the client after updating the duration.
                // Clients.User(userIdString).SendAsync("ReceiveTicketStatus", ticket.TOrderId, "Active", "User has entered museum.");
            }
        }

        // This function will be used to update the user's duration in the museum.
        // It can be called from the client at regular intervals (e.g., every minute or 5 minutes).
        public async Task UpdateUserDurationInMuseum(int ticketOrderId, int minutesSpent)
        {
            var userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) throw new HubException("User not authenticated.");

            var tbUser = await _context.TbUsers.FirstOrDefaultAsync(u => u.AspNetUserId == userIdString && !u.IsDeleted);
            if (tbUser == null) throw new HubException("User record not found.");

            var ticket = await _context.TbBuyAtickets
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(b => b.TOrderId == ticketOrderId && b.UId == tbUser.UId && b.TSucces == "Yes" && b.IsExpiredExplicitly == false);

            if (ticket != null)
            {
                // Update the duration.
                ticket.CurrentDurationMinutes += minutesSpent;

                // Check for expiration based on the ticket's time limit.
                // If TicketLimitHour = 0, consider it to have an unlimited duration.
                var limitMinutes = ticket.Ticket.TicketLimitHour * 60;
                var hasTimeLimit = ticket.Ticket.TicketLimitHour > 0;

                // Also check the 3-day validity from the purchase date.
                var isPastThreeDays = (DateTime.UtcNow - ticket.TCreatedAt).TotalDays >= 3;

                // If the ticket has expired based on duration or date, update IsExpiredExplicitly.
                if ((hasTimeLimit && ticket.CurrentDurationMinutes >= limitMinutes) || isPastThreeDays)
                {
                    ticket.IsExpiredExplicitly = true;
                }

                await _context.SaveChangesAsync();

                // You can send an update to the client about the ticket status.
                await Clients.Caller.SendAsync("ReceiveTicketUpdate", ticket.TOrderId, ticket.CurrentDurationMinutes, ticket.IsExpiredExplicitly);
            }
            else
            {
                // If the ticket isn't found, it might have already expired or doesn't exist.
                // You can send an error message to the client if needed.
                await Clients.Caller.SendAsync("ReceiveTicketUpdate", ticketOrderId, 0, true, "Ticket not found or already expired.");
            }
        }
    }
}