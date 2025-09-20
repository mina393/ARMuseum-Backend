using ARMuseum.Data.Models;
using ARMuseum.Dtos;
using ARMuseum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ARMuseum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // This entire controller is protected and only accessible by Admins.
    public class StatisticsController : ControllerBase
    {
        private readonly OurDbContext _context;

        // Constructor for dependency injection
        public StatisticsController(OurDbContext context)
        {
            _context = context;
        }

        // GET: api/Statistics/users-by-country
        // Returns the number of users grouped by country.
        [HttpGet("users-by-country")]
        public async Task<IActionResult> GetUsersByCountry()
        {
            try
            {
                var stats = await _context.TbUsers
                    .Where(u => !u.IsDeleted && !string.IsNullOrEmpty(u.UCountry))
                    .GroupBy(u => u.UCountry)
                    .Select(g => new UserCountryStatsDto
                    {
                        Country = g.Key,
                        UserCount = g.Count()
                    })
                    .OrderByDescending(s => s.UserCount)
                    .ToListAsync();
                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error in GetUsersByCountry: {ex.Message}");
            }
        }

        // GET: api/Statistics/monthly-revenue
        // Returns the total revenue grouped by month. Can be filtered by country.
        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] string? country)
        {
            try
            {
                var query = _context.TbBuyAtickets
                    .Where(t => t.TSucces.ToLower() == "yes" && t.UIdNavigation != null);

                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(t => t.UIdNavigation.UCountry == country);
                }

                var rawData = await query
                    .GroupBy(t => new { t.TCreatedAt.Year, t.TCreatedAt.Month })
                    .Select(g => new {
                        g.Key.Year,
                        g.Key.Month,
                        Value = g.Sum(t => t.TAmountCents) / 100m
                    })
                    .ToListAsync();

                var stats = rawData
                    .Select(d => new MonthlyStatsDto
                    {
                        Month = $"{d.Year}-{d.Month:D2}",
                        Value = d.Value
                    })
                    .OrderBy(s => s.Month)
                    .ToList();

                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error in GetMonthlyRevenue: {ex.Message}");
            }
        }

        // GET: api/Statistics/sales-by-ticket-type
        // Returns the number of sales grouped by ticket type. Can be filtered by country.
        [HttpGet("sales-by-ticket-type")]
        public async Task<IActionResult> GetSalesByTicketType([FromQuery] string? country)
        {
            try
            {
                var query = _context.TbBuyAtickets
                    .Where(t => t.TSucces.ToLower() == "yes" && t.Ticket != null && t.UIdNavigation != null);

                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(t => t.UIdNavigation.UCountry == country);
                }

                var stats = await query
                    .GroupBy(t => t.Ticket.TicketType)
                    .Select(g => new TicketSalesStatsDto
                    {
                        TicketType = g.Key,
                        SalesCount = g.Count()
                    })
                    .OrderByDescending(s => s.SalesCount)
                    .ToListAsync();
                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error in GetSalesByTicketType: {ex.Message}");
            }
        }

        // GET: api/Statistics/users-by-age-group
        // Returns the number of users grouped by age range. Can be filtered by country.
        [HttpGet("users-by-age-group")]
        public async Task<IActionResult> GetUsersByAgeGroup([FromQuery] string? country)
        {
            try
            {
                var query = _context.TbUsers.Where(u => !u.IsDeleted);

                if (!string.IsNullOrEmpty(country))
                {
                    query = query.Where(u => u.UCountry == country);
                }

                var users = await query.ToListAsync();

                var ageGroups = users
                    .Where(u => u.UDateOfBirth != default(DateTime))
                    .Select(u => new { Age = DateTime.Today.Year - u.UDateOfBirth.Year })
                    .GroupBy(u => GetAgeGroup(u.Age))
                    .Select(g => new AgeGroupStatsDto { AgeGroup = g.Key, UserCount = g.Count() })
                    .OrderBy(g => g.AgeGroup)
                    .ToList();

                return Ok(ageGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error in GetUsersByAgeGroup: {ex.Message}");
            }
        }

        // Helper method to categorize an age into a string representation of an age group.
        private string GetAgeGroup(int age)
        {
            if (age < 5) return "00-04";
            if (age > 99) return "100+";
            int startAge = (age / 5) * 5;
            int endAge = startAge + 4;
            return $"{startAge:D2}-{endAge:D2}";
        }
    }
}