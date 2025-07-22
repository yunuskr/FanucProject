using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanucRelease.Migrations
{
    /// <inheritdoc />
    public partial class IlkYapi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "Users");

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
                name: "KaynakDonguleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaslangicSaati = table.Column<TimeOnly>(type: "time", nullable: false),
                    BitisSaati = table.Column<TimeOnly>(type: "time", nullable: true),
                    ToplamSureSaniye = table.Column<double>(type: "float", nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperatorId = table.Column<int>(type: "int", nullable: false)
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
                    KaynakDongusuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaynakParametreleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaynakParametreleri_KaynakDonguleri_KaynakDongusuId",
                        column: x => x.KaynakDongusuId,
                        principalTable: "KaynakDonguleri",
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
                    KaynakDongusuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakineDuruslari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MakineDuruslari_KaynakDonguleri_KaynakDongusuId",
                        column: x => x.KaynakDongusuId,
                        principalTable: "KaynakDonguleri",
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
                    KaynakDongusuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafoBilgileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafoBilgileri_KaynakDonguleri_KaynakDongusuId",
                        column: x => x.KaynakDongusuId,
                        principalTable: "KaynakDonguleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KaynakDonguleri_OperatorId",
                table: "KaynakDonguleri",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_KaynakParametreleri_KaynakDongusuId",
                table: "KaynakParametreleri",
                column: "KaynakDongusuId");

            migrationBuilder.CreateIndex(
                name: "IX_MakineDuruslari_KaynakDongusuId",
                table: "MakineDuruslari",
                column: "KaynakDongusuId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafoBilgileri_KaynakDongusuId",
                table: "TrafoBilgileri",
                column: "KaynakDongusuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "KaynakParametreleri");

            migrationBuilder.DropTable(
                name: "MakineDuruslari");

            migrationBuilder.DropTable(
                name: "TrafoBilgileri");

            migrationBuilder.DropTable(
                name: "KaynakDonguleri");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Soyad = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }
    }
}
