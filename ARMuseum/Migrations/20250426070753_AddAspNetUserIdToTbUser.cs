using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMuseum.Migrations
{
    /// <inheritdoc />
    public partial class AddAspNetUserIdToTbUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                table: "TbUser",
                type: "nvarchar(450)",
                nullable: true); // false if we want to make sure that every user in TbUser is associated with an Identity account.

            // Add an index to improve search performance.
            migrationBuilder.CreateIndex(
                name: "IX_TbUser_AspNetUserId",
                table: "TbUser",
                column: "AspNetUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                table: "TbUser");
        }
    }
}
