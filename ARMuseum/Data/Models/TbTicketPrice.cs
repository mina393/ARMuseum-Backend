using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[PrimaryKey("TicketId", "TicketDate")]
[Table("TbTicketPrice")]
public partial class TbTicketPrice
{
    [Key]
    [Column("Ticket_Id")]
    public int TicketId { get; set; }

    [Column("Ticket_Price", TypeName = "decimal(10, 4)")]
    public decimal TicketPrice { get; set; }

    [Key]
    [Column("Ticket_Date")]
    public DateOnly TicketDate { get; set; }

    [ForeignKey("TicketId")]
    [InverseProperty("TbTicketPrices")]
    public virtual TbTicket Ticket { get; set; } = null!;
}
