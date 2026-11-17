using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLicencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WIS_LICENCIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Serial = table.Column<long>(type: "bigint", nullable: false),
                    Cliente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Plan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Edicion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaxEmpresas = table.Column<int>(type: "int", nullable: false),
                    MaxUsuarios = table.Column<int>(type: "int", nullable: false),
                    Emitida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GraceDays = table.Column<int>(type: "int", nullable: false),
                    MaxVersionMayor = table.Column<int>(type: "int", nullable: true),
                    Archivo = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CargadaUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CargadaPor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_LICENCIA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WIS_LICENCIA_INSTALACION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstallationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UltimoVisto = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_LICENCIA_INSTALACION", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WIS_LICENCIA_LicenseId_Serial",
                table: "WIS_LICENCIA",
                columns: new[] { "LicenseId", "Serial" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WIS_LICENCIA");

            migrationBuilder.DropTable(
                name: "WIS_LICENCIA_INSTALACION");
        }
    }
}
