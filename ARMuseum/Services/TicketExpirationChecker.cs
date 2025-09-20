// File: ARMuseum/Services/TicketExpirationChecker.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ARMuseum.Data.Models; // Ensure correct namespace for the DbContext
using ARMuseum.Models;      // Ensure correct namespace for UserTicketDto (if used)
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Services
{
    public class TicketExpirationChecker : BackgroundService
    {
        private readonly ILogger<TicketExpirationChecker> _logger;
        private readonly IServiceProvider _serviceProvider; // Used to create scoped services like DbContext

        public TicketExpirationChecker(ILogger<TicketExpirationChecker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Ticket Expiration Checker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Ticket Expiration Checker working at: {time}", DateTimeOffset.Now);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<OurDbContext>();
                    await CheckAndExpireTickets(dbContext);
                }

                // Run this job every 15 minutes (or as needed).
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }

            _logger.LogInformation("Ticket Expiration Checker is stopping.");
        }

        private async Task CheckAndExpireTickets(OurDbContext dbContext)
        {
            var currentUtcTime = DateTime.UtcNow;
            _logger.LogInformation("Checking for expired tickets at UTC: {utcTime}", currentUtcTime);

            // Fetch tickets that have not yet been explicitly marked as expired.
            var ticketsToEvaluate = await dbContext.TbBuyAtickets
                .Where(b => b.TSucces == "Yes" && b.IsExpiredExplicitly == false)
                .Include(b => b.Ticket) // We need the Ticket_Limit_Hour data.
                .ToListAsync();

            int expiredCount = 0;
            foreach (var ticket in ticketsToEvaluate)
            {
                var expirationDateByTime = ticket.Ticket.TicketLimitHour > 0
                    ? ticket.TCreatedAt.AddHours(ticket.Ticket.TicketLimitHour)
                    : ticket.TCreatedAt.AddDays(3);

                // Check expiration conditions.
                bool isExpiredByDate = expirationDateByTime < currentUtcTime;
                bool isExpiredByDuration = ticket.Ticket.TicketLimitHour > 0 &&
                                           ticket.CurrentDurationMinutes >= (ticket.Ticket.TicketLimitHour * 60);

                if (isExpiredByDate || isExpiredByDuration)
                {
                    ticket.IsExpiredExplicitly = true; // Update the expiration status on the object.
                    expiredCount++;
                }
            }

            if (expiredCount > 0)
            {
                await dbContext.SaveChangesAsync(); // Save changes to the database.
                _logger.LogInformation("Updated {count} tickets to IsExpiredExplicitly = true.", expiredCount);
            }
            else
            {
                _logger.LogInformation("No new tickets found to expire.");
            }
        }
    }
}