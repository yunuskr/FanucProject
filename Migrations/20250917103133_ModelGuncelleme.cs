using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class ModelGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hata_ProgramVerileri_ProgramVerisiId",
                table: "Hata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hata",
                table: "Hata");

            migrationBuilder.RenameTable(
                name: "Hata",
                newName: "Hatalar");

            migrationBuilder.RenameIndex(
                name: "IX_Hata_ProgramVerisiId",
                table: "Hatalar",
                newName: "IX_Hatalar_ProgramVerisiId");

            migrationBuilder.AddColumn<string>(
                name: "Kod",
                table: "Hatalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hatalar",
                table: "Hatalar",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hatalar_ProgramVerileri_ProgramVerisiId",
                table: "Hatalar",
                column: "ProgramVerisiId",
                principalTable: "ProgramVerileri",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hatalar_ProgramVerileri_ProgramVerisiId",
                table: "Hatalar");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hatalar",
                table: "Hatalar");

            migrationBuilder.DropColumn(
                name: "Kod",
                table: "Hatalar");

            migrationBuilder.RenameTable(
                name: "Hatalar",
                newName: "Hata");

            migrationBuilder.RenameIndex(
                name: "IX_Hatalar_ProgramVerisiId",
                table: "Hata",
                newName: "IX_Hata_ProgramVerisiId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hata",
                table: "Hata",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hata_ProgramVerileri_ProgramVerisiId",
                table: "Hata",
                column: "ProgramVerisiId",
                principalTable: "ProgramVerileri",
                principalColumn: "Id");
        }
    }
}
