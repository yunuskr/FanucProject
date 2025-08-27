using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class MakeProgramVerisiNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RobotVerileri_Kaynaklar_KaynakId",
                table: "RobotVerileri");

            migrationBuilder.DropForeignKey(
                name: "FK_RobotVerileri_Operators_OperatorId",
                table: "RobotVerileri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RobotVerileri",
                table: "RobotVerileri");

            migrationBuilder.RenameTable(
                name: "RobotVerileri",
                newName: "ProgramVerileri");

            migrationBuilder.RenameIndex(
                name: "IX_RobotVerileri_OperatorId",
                table: "ProgramVerileri",
                newName: "IX_ProgramVerileri_OperatorId");

            migrationBuilder.RenameIndex(
                name: "IX_RobotVerileri_KaynakId",
                table: "ProgramVerileri",
                newName: "IX_ProgramVerileri_KaynakId");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "ProgramVerileri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "KaynakId",
                table: "ProgramVerileri",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgramVerileri",
                table: "ProgramVerileri",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramVerileri_Kaynaklar_KaynakId",
                table: "ProgramVerileri",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramVerileri_Operators_OperatorId",
                table: "ProgramVerileri",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgramVerileri_Kaynaklar_KaynakId",
                table: "ProgramVerileri");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramVerileri_Operators_OperatorId",
                table: "ProgramVerileri");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgramVerileri",
                table: "ProgramVerileri");

            migrationBuilder.RenameTable(
                name: "ProgramVerileri",
                newName: "RobotVerileri");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramVerileri_OperatorId",
                table: "RobotVerileri",
                newName: "IX_RobotVerileri_OperatorId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgramVerileri_KaynakId",
                table: "RobotVerileri",
                newName: "IX_RobotVerileri_KaynakId");

            migrationBuilder.AlterColumn<int>(
                name: "OperatorId",
                table: "RobotVerileri",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "KaynakId",
                table: "RobotVerileri",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RobotVerileri",
                table: "RobotVerileri",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RobotVerileri_Kaynaklar_KaynakId",
                table: "RobotVerileri",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RobotVerileri_Operators_OperatorId",
                table: "RobotVerileri",
                column: "OperatorId",
                principalTable: "Operators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
