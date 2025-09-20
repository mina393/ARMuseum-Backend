using System;
using System.ComponentModel.DataAnnotations;

namespace ARMuseum.Dtos
{
    public class CallbackRequestDto
    {
        [Required]
        public int MId { get; set; }

        [Required]
        public int TicketId { get; set; }

        [Required]
        public int UId { get; set; }

        [Required]
        public int TOrderId { get; set; }

        [Required]
        public DateTime TCreatedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string TCurrency { get; set; }

        [Required]
        [StringLength(10)]
        public string TIsRefund { get; set; }

        [Required]
        [StringLength(10)]
        public string TSucces { get; set; }

        [Required]
        public decimal TAmountCents { get; set; }

        [Required]
        public string Hmac { get; set; } 
    }
}
