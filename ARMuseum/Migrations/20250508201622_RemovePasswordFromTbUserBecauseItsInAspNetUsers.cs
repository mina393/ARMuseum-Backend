using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMuseum.Migrations
{
    /// <inheritdoc />
    public partial class RemovePasswordFromTbUserBecauseItsInAspNetUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "U_Password",
                table: "TbUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "U_Password",
                table: "TbUser",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
