using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[Table("TbStatue")]
public partial class TbStatue
{
    [Key]
    [Column("S_Id")]
    public int SId { get; set; }

    [Column("S_Name")]
    [StringLength(50)]
    public string SName { get; set; } = null!;

    [Column("S_Birth_Date", TypeName = "datetime")]
    public DateTime? SBirthDate { get; set; }

    [Column("S_Death_Date", TypeName = "datetime")]
    public DateTime? SDeathDate { get; set; }

    [Column("S_Story")]
    public string SStory { get; set; } = null!;

    [Column("M_Id")]
    public int MId { get; set; }

    [Column("S_3d_Model_Name")]
    public string S3dModelName { get; set; } = null!;

    [Column("S_Live_Face_Name")]
    public string? SLiveFaceName { get; set; }

    [ForeignKey("MId")]
    [InverseProperty("TbStatues")]
    public virtual TbMuseum MIdNavigation { get; set; } = null!;

    [InverseProperty("SIdNavigation")]
    public virtual ICollection<TbCategory> TbCategories { get; set; } = new List<TbCategory>();

    [InverseProperty("SIdNavigation")]
    public virtual ICollection<TbStatueVideo> TbStatueVideos { get; set; } = new List<TbStatueVideo>();
}
