namespace ARMuseum.Models
{
    // DTO for representing user statistics grouped by country.
    public class UserCountryStatsDto
    {
        public string Country { get; set; }
        public int UserCount { get; set; }
    }

    // DTO for representing monthly statistics (e.g., new users or revenue).
    public class MonthlyStatsDto
    {
        public string Month { get; set; } // Format: "YYYY-MM"
        public decimal Value { get; set; } // This value can represent either a count or a monetary amount.
    }

    // DTO for representing ticket sales statistics grouped by ticket type.
    public class TicketSalesStatsDto
    {
        public string TicketType { get; set; }
        public int SalesCount { get; set; }
    }

    // DTO for representing user statistics grouped by age.
    public class AgeGroupStatsDto
    {
        public string AgeGroup { get; set; }
        public int UserCount { get; set; }
    }
}