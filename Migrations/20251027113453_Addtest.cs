using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class Addtest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kaynakAnindaMi",
                table: "Hatalar");

            migrationBuilder.AddColumn<string>(
                name: "KaynakAdi",
                table: "Kaynaklar",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hatalar_Kaynaklar_KaynakId",
                table: "Hatalar");

            migrationBuilder.DropIndex(
                name: "IX_Hatalar_KaynakId",
                table: "Hatalar");

            migrationBuilder.DropColumn(
                name: "KaynakAdi",
                table: "Kaynaklar");

            migrationBuilder.DropColumn(
                name: "KaynakId",
                table: "Hatalar");

            migrationBuilder.AddColumn<bool>(
                name: "kaynakAnindaMi",
                table: "Hatalar",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
