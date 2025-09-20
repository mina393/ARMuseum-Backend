using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[PrimaryKey("MId", "MDepartments")]
public partial class TbMuseumDepartment
{
    [Key]
    [Column("M_Id")]
    public int MId { get; set; }

    [Key]
    [Column("M_Departments")]
    [StringLength(300)]
    public string MDepartments { get; set; } = null!;

    [ForeignKey("MId")]
    [InverseProperty("TbMuseumDepartments")]
    public virtual TbMuseum MIdNavigation { get; set; } = null!;
}
