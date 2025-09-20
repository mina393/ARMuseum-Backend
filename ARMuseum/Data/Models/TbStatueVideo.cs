using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[PrimaryKey("SId", "VideoId")]
public partial class TbStatueVideo
{
    [Key]
    [Column("S_Id")]
    public int SId { get; set; }

    [Column("S_Videos")]
    public byte[]? SVideos { get; set; }

    [Key]
    [Column("Video_Id")]
    public int VideoId { get; set; }

    [ForeignKey("SId")]
    [InverseProperty("TbStatueVideos")]
    public virtual TbStatue SIdNavigation { get; set; } = null!;
}
