using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Models
{
    namespace ARMuseum.Dtos
    {
        // Defines the data structure for ticket information displayed to an admin.
        public class TicketAdminDto
        {
            public int TicketId { get; set; }
            public string TicketType { get; set; }
            public int TicketLimitHour { get; set; }
            public string TicketDescription { get; set; }
            public decimal CurrentPrice { get; set; } // The current price of the ticket.
        }

        // Defines the data structure for updating a ticket.
        public class UpdateTicketDto
        {
            [Required]
            public int TicketLimitHour { get; set; }

            [Required]
            [Range(0.01, double.MaxValue)]
            public decimal NewPrice { get; set; }
        }
    }
}