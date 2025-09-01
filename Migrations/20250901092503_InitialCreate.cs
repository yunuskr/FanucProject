using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramVerileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProgramAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KaynakSayisi = table.Column<int>(type: "int", nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramVerileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramVerileri_Operators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Operators",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Kaynaklar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaslangicSaati = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisSaati = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplamSure = table.Column<TimeOnly>(type: "time", nullable: false),
                    BaslangicSatiri = table.Column<int>(type: "int", nullable: false),
                    KaynakUzunlugu = table.Column<int>(type: "int", nullable: false),
                    BitisSatiri = table.Column<int>(type: "int", nullable: false),
                    PrcNo = table.Column<int>(type: "int", nullable: false),
                    SrcNo = table.Column<int>(type: "int", nullable: false),
                    ProgramVerisiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kaynaklar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kaynaklar_ProgramVerileri_ProgramVerisiId",
                        column: x => x.ProgramVerisiId,
                        principalTable: "ProgramVerileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "KaynakParametreleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OlcumZamani = table.Column<TimeOnly>(type: "time", nullable: false),
                    Voltaj = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amper = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TelSurmeHizi = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaynakParametreleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaynakParametreleri_Kaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "Kaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MakineDuruslari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaslangicSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    Sebep = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakineDuruslari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakineDuruslari_Kaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "Kaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrafoBilgileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrafoAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KaynakId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafoBilgileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafoBilgileri_Kaynaklar_KaynakId",
                        column: x => x.KaynakId,
                        principalTable: "Kaynaklar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnlikKaynaklar_KaynakId",
                table: "AnlikKaynaklar",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_Kaynaklar_ProgramVerisiId",
                table: "Kaynaklar",
                column: "ProgramVerisiId");

            migrationBuilder.CreateIndex(
                name: "IX_KaynakParametreleri_KaynakId",
                table: "KaynakParametreleri",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_MakineDuruslari_KaynakId",
                table: "MakineDuruslari",
                column: "KaynakId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramVerileri_OperatorId",
                table: "ProgramVerileri",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafoBilgileri_KaynakId",
                table: "TrafoBilgileri",
                column: "KaynakId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "AnlikKaynaklar");

            migrationBuilder.DropTable(
                name: "KaynakParametreleri");

            migrationBuilder.DropTable(
                name: "MakineDuruslari");

            migrationBuilder.DropTable(
                name: "TrafoBilgileri");

            migrationBuilder.DropTable(
                name: "Kaynaklar");

            migrationBuilder.DropTable(
                name: "ProgramVerileri");

            migrationBuilder.DropTable(
                name: "Operators");
        }
    }
}
