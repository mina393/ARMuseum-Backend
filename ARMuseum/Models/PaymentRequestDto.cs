using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Dtos
{
    public class PaymentRequestDto
    {
        [Required]
        public int MId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int UId { get; set; }

        [Required]
        [StringLength(50)]
        public string Currency { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal AmountCents { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }
    }
}
