using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarBloqueoOrdenes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WIS_ORDER_LOCK",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    IdOrder = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Expira = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Actualizado = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WIS_ORDER_LOCK", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WIS_ORDER_LOCK_EmpresaId_IdOrder",
                table: "WIS_ORDER_LOCK",
                columns: new[] { "EmpresaId", "IdOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WIS_ORDER_LOCK");
        }
    }
}
