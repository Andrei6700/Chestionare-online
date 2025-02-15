using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chestionare_online.Migrations
{
    /// <inheritdoc />
    public partial class AddExamQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "table_B_B1",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    intrebare = table.Column<string>(type: "text", nullable: false),
                    varianta_a = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    varianta_b = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    varianta_c = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageURL = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table_B_B1", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "table_B_B1");
        }
    }
}
