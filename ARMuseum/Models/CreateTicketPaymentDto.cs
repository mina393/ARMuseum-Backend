using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Dtos
{
    public class CreateTicketPaymentDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Date should be in yyyy-MM-dd format.")]
        public string TicketDate { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be 3 letters")]
        public string Currency { get; set; } = "egp";

        [Required]
        public int MuseumId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}