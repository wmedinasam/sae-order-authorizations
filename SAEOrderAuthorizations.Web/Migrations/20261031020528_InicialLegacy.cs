using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class InicialLegacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMPRESA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DB_SAE = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sufijo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMPRESA", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EsAdministrador = table.Column<bool>(type: "bit", nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROL_EMPRESA",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROL_EMPRESA", x => new { x.RolId, x.EmpresaId });
                });

            migrationBuilder.CreateTable(
                name: "WIS_CONFIGURATION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KEY = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    VALUE = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_CONFIGURATION", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WIS_Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    IdOrder = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdSupplier = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OrdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Total = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WIS_USER",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FULL_NAME = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NICK_NAME = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    PASSWORD = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EMAIL = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SEND_MAIL = table.Column<bool>(type: "bit", nullable: true),
                    ROL_ID = table.Column<int>(type: "int", nullable: true),
                    ACTIVATED_DATE = table.Column<DateTime>(type: "datetime2", nullable: true),
                    USER_ACTIVATES = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_USER", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WIS_WorkLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: true),
                    CorrelatorId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdOrder = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Descr = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdSupplier = table.Column<string>(type: "nchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_WorkLog", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMPRESA");

            migrationBuilder.DropTable(
                name: "ROL");

            migrationBuilder.DropTable(
                name: "ROL_EMPRESA");

            migrationBuilder.DropTable(
                name: "WIS_CONFIGURATION");

            migrationBuilder.DropTable(
                name: "WIS_Orders");

            migrationBuilder.DropTable(
                name: "WIS_USER");

            migrationBuilder.DropTable(
                name: "WIS_WorkLog");
        }
    }
}
