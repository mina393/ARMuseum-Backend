namespace ARMuseum.Models;

using System;

// DTO for representing a user's purchased ticket with all relevant details.
public class UserTicketDto
{
    public int OrderId { get; set; } // From TbBuyAticket (T_Order_Id)
    public int TicketId { get; set; } // From TbBuyAticket
    public int MuseumId { get; set; } // From TbBuyAticket (M_Id)

    public string MuseumName { get; set; } = null!; // From the related TbMuseum entity
    public string MuseumImageUrl { get; set; } = null!; // From TbMuseum (constructed as a full URL)

    public string TicketType { get; set; } = null!; // From the related TbTicket entity
    public string TicketDescription { get; set; } = null!; // From the related TbTicket entity
    public decimal Price { get; set; } // From TbTicketPrice or TbBuyAticket
    public string Currency { get; set; } = null!; // From TbBuyAticket

    public DateTime PurchaseDate { get; set; } // From TbBuyAticket (T_Created_AT)
    public int TicketLimitHours { get; set; } // From TbTicket (to determine validity duration in hours)

    // Calculated expiration info
    public DateTime ExpirationDate { get; set; } // The calculated expiration date
    public TimeSpan TimeLeft { get; set; } // The remaining time until expiration

    public string Status { get; set; } = null!; // The ticket's current status (e.g., Active, Expired)

    public int CurrentDurationMinutes { get; set; }
    public bool IsExpiredExplicitly { get; set; }
}