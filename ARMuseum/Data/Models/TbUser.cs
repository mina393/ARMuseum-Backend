using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ARMuseum.Data.Models;

[Table("TbUser")]
public partial class TbUser
{
    [Key]
    [Column("U_Id")]
    public int UId { get; set; }

    [Column("U_UserName")]
    [StringLength(50)]
    public string UUserName { get; set; } = null!;

    [Column("U_DateOfBirth", TypeName = "datetime")]
    public DateTime UDateOfBirth { get; set; }

    [Column("U_FirstName")]
    [StringLength(100)]
    public string UFirstName { get; set; } = null!;

    [Column("U_LastName")]
    [StringLength(100)]
    public string ULastName { get; set; } = null!;

    //[Column("U_Password")]
    //[StringLength(200)] // اضف طول مناسب
    //public string UPassword { get; set; } = null!;

    [Column("U_Phone")]
    [StringLength(100)]
    public string UPhone { get; set; } = null!;

    [Column("U_Country")]
    [StringLength(500)]
    public string UCountry { get; set; } = null!;

    [Column("U_Email")]
    [StringLength(300)]
    public string UEmail { get; set; } = null!;

    [Column("U_Image_Name")]
    public string? UImageName { get; set; }
    [Column("U_Is_Deleted")]
    public bool IsDeleted { get; set; } = false;

    [StringLength(450)]
    [ForeignKey("AspNetUsers")]
    public string? AspNetUserId { get; set; }

    [InverseProperty("UIdNavigation")]
    public virtual ICollection<TbBuyAticket> TbBuyAtickets { get; set; } = new List<TbBuyAticket>();
}
