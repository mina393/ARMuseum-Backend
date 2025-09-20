using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[Table("TbTicket")]
public partial class TbTicket
{
    [Key]
    [Column("Ticket_Id")]
    public int TicketId { get; set; }

    [Column("Ticket_Type")]
    [StringLength(200)]
    public string TicketType { get; set; } = null!;

    [Column("Ticket_Limit_Hour")]
    public int TicketLimitHour { get; set; }
   
    [Column("Ticket_Description", TypeName = "nvarchar(MAX)")]
    [Required] 
    public string TicketDescription { get; set; } = string.Empty; // لتحديد القيمة الابتدائية


    [InverseProperty("Ticket")]
    public virtual ICollection<TbBuyAticket> TbBuyAtickets { get; set; } = new List<TbBuyAticket>();

    [InverseProperty("Ticket")]
    public virtual ICollection<TbTicketPrice> TbTicketPrices { get; set; } = new List<TbTicketPrice>();
}
