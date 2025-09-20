using System.ComponentModel.DataAnnotations;

public class BuyTicketRequestDto
{
    [Required]
    public int TicketId { get; set; }
    [Required]
    public string UId { get; set; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    [Required]
    public int MId { get; set; }
    [Required]
    public string Currency { get; set; } = "EGP";
}