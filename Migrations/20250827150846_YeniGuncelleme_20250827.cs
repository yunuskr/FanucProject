using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class YeniGuncelleme_20250827 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "ProgramVerileri");

            migrationBuilder.DropColumn(
                name: "HataKodu",
                table: "ProgramVerileri");

            migrationBuilder.DropColumn(
                name: "ToplamSureSaniye",
                table: "Kaynaklar");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "BitisSaati",
                table: "Kaynaklar",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0),
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ToplamSure",
                table: "Kaynaklar",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToplamSure",
                table: "Kaynaklar");

            migrationBuilder.AddColumn<string>(
                name: "Durum",
                table: "ProgramVerileri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HataKodu",
                table: "ProgramVerileri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "BitisSaati",
                table: "Kaynaklar",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time");

            migrationBuilder.AddColumn<double>(
                name: "ToplamSureSaniye",
                table: "Kaynaklar",
                type: "float",
                nullable: true);
        }
    }
}
