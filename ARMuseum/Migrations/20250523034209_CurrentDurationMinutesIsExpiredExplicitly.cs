using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMuseum.Migrations
{
    /// <inheritdoc />
    public partial class CurrentDurationMinutesIsExpiredExplicitly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Current_Duration_Minutes",
                table: "TbBuyATicket",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Is_Expired_Explicitly",
                table: "TbBuyATicket",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Current_Duration_Minutes",
                table: "TbBuyATicket");

            migrationBuilder.DropColumn(
                name: "Is_Expired_Explicitly",
                table: "TbBuyATicket");
        }
    }
}
