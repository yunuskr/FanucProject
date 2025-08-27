using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class YeniModelGuncellemesi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KaynakParametreleri_KaynakDonguleri_KaynakDongusuId",
                table: "KaynakParametreleri");

            migrationBuilder.DropForeignKey(
                name: "FK_MakineDuruslari_KaynakDonguleri_KaynakDongusuId",
                table: "MakineDuruslari");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafoBilgileri_KaynakDonguleri_KaynakDongusuId",
                table: "TrafoBilgileri");

            migrationBuilder.DropTable(
                name: "KaynakDonguleri");

            migrationBuilder.RenameColumn(
                name: "KaynakDongusuId",
                table: "TrafoBilgileri",
                newName: "KaynakId");

            migrationBuilder.RenameIndex(
                name: "IX_TrafoBilgileri_KaynakDongusuId",
                table: "TrafoBilgileri",
                newName: "IX_TrafoBilgileri_KaynakId");

            migrationBuilder.RenameColumn(
                name: "KaynakDongusuId",
                table: "MakineDuruslari",
                newName: "KaynakId");

            migrationBuilder.RenameIndex(
                name: "IX_MakineDuruslari_KaynakDongusuId",
                table: "MakineDuruslari",
                newName: "IX_MakineDuruslari_KaynakId");

            migrationBuilder.RenameColumn(
                name: "KaynakDongusuId",
                table: "KaynakParametreleri",
                newName: "KaynakId");

            migrationBuilder.RenameIndex(
                name: "IX_KaynakParametreleri_KaynakDongusuId",
                table: "KaynakParametreleri",
                newName: "IX_KaynakParametreleri_KaynakId");

            migrationBuilder.CreateTable(
                name: "Kaynaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaslangicSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    BitisSaati = table.Column<TimeOnly>(type: "time", nullable: true),
                    ToplamSureSaniye = table.Column<double>(type: "float", nullable: true),
                    BaslangicSatiri = table.Column<int>(type: "int", nullable: false),
                    BitisSatiri = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kaynaklar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnlikKaynaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OlcumZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Voltaj = table.Column<double>(type: "float", nullable: false),
                    Amper = table.Column<double>(type: "float", nullable: false),
                    TelSurmeHizi = table.Column<double>(type: "float", nullable: false),
                    KaynakHizi = table.Column<int>(type: "int", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnlikKaynaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnlikKaynaklar_Kaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "Kaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RobotVerileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HataKodu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KaynakSayisi = table.Column<int>(type: "int", nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RobotVerileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RobotVerileri_Kaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "Kaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RobotVerileri_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnlikKaynaklar_KaynakId",
                table: "AnlikKaynaklar",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotVerileri_KaynakId",
                table: "RobotVerileri",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotVerileri_OperatorId",
                table: "RobotVerileri",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_KaynakParametreleri_Kaynaklar_KaynakId",
                table: "KaynakParametreleri",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MakineDuruslari_Kaynaklar_KaynakId",
                table: "MakineDuruslari",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafoBilgileri_Kaynaklar_KaynakId",
                table: "TrafoBilgileri",
                column: "KaynakId",
                principalTable: "Kaynaklar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KaynakParametreleri_Kaynaklar_KaynakId",
                table: "KaynakParametreleri");

            migrationBuilder.DropForeignKey(
                name: "FK_MakineDuruslari_Kaynaklar_KaynakId",
                table: "MakineDuruslari");

            migrationBuilder.DropForeignKey(
                name: "FK_TrafoBilgileri_Kaynaklar_KaynakId",
                table: "TrafoBilgileri");

            migrationBuilder.DropTable(
                name: "AnlikKaynaklar");

            migrationBuilder.DropTable(
                name: "RobotVerileri");

            migrationBuilder.DropTable(
                name: "Kaynaklar");

            migrationBuilder.RenameColumn(
                name: "KaynakId",
                table: "TrafoBilgileri",
                newName: "KaynakDongusuId");

            migrationBuilder.RenameIndex(
                name: "IX_TrafoBilgileri_KaynakId",
                table: "TrafoBilgileri",
                newName: "IX_TrafoBilgileri_KaynakDongusuId");

            migrationBuilder.RenameColumn(
                name: "KaynakId",
                table: "MakineDuruslari",
                newName: "KaynakDongusuId");

            migrationBuilder.RenameIndex(
                name: "IX_MakineDuruslari_KaynakId",
                table: "MakineDuruslari",
                newName: "IX_MakineDuruslari_KaynakDongusuId");

            migrationBuilder.RenameColumn(
                name: "KaynakId",
                table: "KaynakParametreleri",
                newName: "KaynakDongusuId");

            migrationBuilder.RenameIndex(
                name: "IX_KaynakParametreleri_KaynakId",
                table: "KaynakParametreleri",
                newName: "IX_KaynakParametreleri_KaynakDongusuId");

            migrationBuilder.CreateTable(
                name: "KaynakDonguleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperatorId = table.Column<int>(type: "int", nullable: false),
                    BaslangicSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    BitisSaati = table.Column<TimeOnly>(type: "time", nullable: true),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    ToplamSureSaniye = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaynakDonguleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaynakDonguleri_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KaynakDonguleri_OperatorId",
                table: "KaynakDonguleri",
                column: "OperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_KaynakParametreleri_KaynakDonguleri_KaynakDongusuId",
                table: "KaynakParametreleri",
                column: "KaynakDongusuId",
                principalTable: "KaynakDonguleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MakineDuruslari_KaynakDonguleri_KaynakDongusuId",
                table: "MakineDuruslari",
                column: "KaynakDongusuId",
                principalTable: "KaynakDonguleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrafoBilgileri_KaynakDonguleri_KaynakDongusuId",
                table: "TrafoBilgileri",
                column: "KaynakDongusuId",
                principalTable: "KaynakDonguleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
