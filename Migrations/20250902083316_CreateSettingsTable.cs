using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class CreateSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RobotIp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RobotUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RobotPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SqlIp = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Database = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SqlUser = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SqlPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrustServerCertificate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
