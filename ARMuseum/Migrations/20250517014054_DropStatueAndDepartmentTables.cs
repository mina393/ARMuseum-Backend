using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARMuseum.Migrations
{
    /// <inheritdoc />
    public partial class DropStatueAndDepartmentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // حذف الجداول المرتبطة أولًا لتجنب مشاكل المفاتيح الخارجية
            migrationBuilder.DropTable(name: "TbStatueVideos");
            migrationBuilder.DropTable(name: "TbCategory");
            migrationBuilder.DropTable(name: "TbStatue");
            migrationBuilder.DropTable(name: "TbMuseumDepartments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // في حال الرجوع للوراء، يتم إعادة إنشاء الجداول (اختياريًا يمكن تركها فاضية أو حذفها تمامًا لو مش محتاج rollback)
            migrationBuilder.CreateTable(
                name: "TbMuseumDepartments",
                columns: table => new
                {
                    M_Id = table.Column<int>(type: "int", nullable: false),
                    M_Departments = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbMuseumDepartments", x => new { x.M_Id, x.M_Departments });
                    table.ForeignKey(
                        name: "FK_TbMuseumDepartments_TbMuseum",
                        column: x => x.M_Id,
                        principalTable: "TbMuseum",
                        principalColumn: "M_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TbStatue",
                columns: table => new
                {
                    S_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    M_Id = table.Column<int>(type: "int", nullable: false),
                    // Add other columns if needed
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbStatue", x => x.S_Id);
                    table.ForeignKey(
                        name: "FK_TbStatue_TbMuseum",
                        column: x => x.M_Id,
                        principalTable: "TbMuseum",
                        principalColumn: "M_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TbCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    S_Id = table.Column<int>(type: "int", nullable: false),
                    // Add other columns if needed
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TbCategory_TbStatue",
                        column: x => x.S_Id,
                        principalTable: "TbStatue",
                        principalColumn: "S_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TbStatueVideos",
                columns: table => new
                {
                    S_Id = table.Column<int>(type: "int", nullable: false),
                    Video_Id = table.Column<int>(type: "int", nullable: false),
                    // Add other columns if needed
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbStatueVideos", x => new { x.S_Id, x.Video_Id });
                    table.ForeignKey(
                        name: "FK_TbStatueVideos_TbStatue",
                        column: x => x.S_Id,
                        principalTable: "TbStatue",
                        principalColumn: "S_Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
