using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class AddTarihToProgramVerisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Tarih",
                table: "ProgramVerileri",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tarih",
                table: "ProgramVerileri");
        }
    }
}
