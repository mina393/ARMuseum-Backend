namespace ARMuseum.Models
{
    public class TicketOptionDto
    {
        public int TicketId { get; set; }
        public string TicketType { get; set; } = null!;
        public string TicketDescription { get; set; } = null!;
        public decimal Price { get; set; } 
        public string Currency { get; set; } = "EGP"; 
    }
}
