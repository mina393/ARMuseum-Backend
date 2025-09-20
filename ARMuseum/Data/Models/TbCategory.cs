using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[PrimaryKey("SId", "CName")]
[Table("TbCategory")]
public partial class TbCategory
{
    [Key]
    [Column("S_Id")]
    public int SId { get; set; }

    [Key]
    [Column("C_Name")]
    [StringLength(200)]
    public string CName { get; set; } = null!;

    [Column("C_Period_Time")]
    [StringLength(30)]
    public string? CPeriodTime { get; set; }

    [ForeignKey("SId")]
    [InverseProperty("TbCategories")]
    public virtual TbStatue SIdNavigation { get; set; } = null!;
}