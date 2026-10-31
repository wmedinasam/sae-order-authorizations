using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class SemillaAprobador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ROL",
                columns: new[] { "Id", "Descripcion", "EsAdministrador", "Estado", "Nombre" },
                values: new object[] { 3, "Aprobador de órdenes", false, true, "Aprobador" });

            migrationBuilder.InsertData(
                table: "ROL_EMPRESA",
                columns: new[] { "EmpresaId", "RolId" },
                values: new object[] { 1, 3 });

            migrationBuilder.InsertData(
                table: "WIS_USER",
                columns: new[] { "Id", "ACTIVATED_DATE", "EMAIL", "FULL_NAME", "NICK_NAME", "PASSWORD", "ROL_ID", "SEND_MAIL", "USER_ACTIVATES" },
                values: new object[] { 3, new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "aprobador@admin.com", "aprobador", "aprobador", "123", 3, false, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ROL",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ROL_EMPRESA",
                keyColumns: new[] { "EmpresaId", "RolId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
