using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class hataupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hatalar_Kaynaklar_KaynakId",
                table: "Hatalar");

            migrationBuilder.DropIndex(
                name: "IX_Hatalar_KaynakId",
                table: "Hatalar");

            migrationBuilder.DropColumn(
                name: "KaynakId",
                table: "Hatalar");

            migrationBuilder.RenameColumn(
                name: "kaynakAnindaMi",
                table: "Hatalar",
                newName: "KaynakAnindaMi");

            migrationBuilder.AddColumn<string>(
                name: "KaynakAdi",
                table: "Hatalar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KaynakAdi",
                table: "Hatalar");

            migrationBuilder.RenameColumn(
                name: "KaynakAnindaMi",
                table: "Hatalar",
                newName: "kaynakAnindaMi");

            migrationBuilder.AddColumn<int>(
                name: "KaynakId",
                table: "Hatalar",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hatalar_KaynakId",
                table: "Hatalar",
                column: "KaynakId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hatalar_Kaynaklar_KaynakId",
                table: "Hatalar",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id");
        }
    }
}
