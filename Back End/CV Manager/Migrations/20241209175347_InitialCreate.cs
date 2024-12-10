using Microsoft.EntityFrameworkCore.Migrations;

namespace CV_Manager.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalInformation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalInformation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CVs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Personal_Information_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CVs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CVs_PersonalInformation_Personal_Information_Id",
                        column: x => x.Personal_Information_Id,
                        principalTable: "PersonalInformation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExperienceInformation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompanyField = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CVId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperienceInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperienceInformation_CVs_CVId",
                        column: x => x.CVId,
                        principalTable: "CVs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CVs_Personal_Information_Id",
                table: "CVs",
                column: "Personal_Information_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExperienceInformation_CVId",
                table: "ExperienceInformation",
                column: "CVId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExperienceInformation");

            migrationBuilder.DropTable(
                name: "CVs");

            migrationBuilder.DropTable(
                name: "PersonalInformation");
        }
    }
}
