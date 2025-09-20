using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[Table("TbMuseum")]
public partial class TbMuseum
{
    [Key]
    [Column("M_Id")]
    public int MId { get; set; }

    [Column("M_History")]
    public string MHistory { get; set; } = null!;

    [Column("M_Name")]
    [StringLength(50)]
    public string MName { get; set; } = null!;

    [Column("M_Map_Name")]
    public string? MMapName { get; set; }

    // Add the new column for the museum image name
    [Column("M_Image_Name")]
    [StringLength(255)] // Choose an appropriate length for your image file names
    public string MImageName { get; set; } = null!;

    [InverseProperty("MIdNavigation")]
    public virtual ICollection<TbBuyAticket> TbBuyAtickets { get; set; } = new List<TbBuyAticket>();

    [InverseProperty("MIdNavigation")]
    public virtual ICollection<TbMuseumDepartment> TbMuseumDepartments { get; set; } = new List<TbMuseumDepartment>();

    [InverseProperty("MIdNavigation")]
    public virtual ICollection<TbStatue> TbStatues { get; set; } = new List<TbStatue>();
}