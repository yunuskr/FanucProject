using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class AddHataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hata",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Zaman = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgramVerisiId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hata_ProgramVerileri_ProgramVerisiId",
                        column: x => x.ProgramVerisiId,
                        principalTable: "ProgramVerileri",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hata_ProgramVerisiId",
                table: "Hata",
                column: "ProgramVerisiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hata");
        }
    }
}
