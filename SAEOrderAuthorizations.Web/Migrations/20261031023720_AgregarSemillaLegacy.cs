using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSemillaLegacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ROL",
                columns: new[] { "Id", "Descripcion", "EsAdministrador", "Estado", "Nombre" },
                values: new object[] { 1, "Administrador", true, true, "Admin" });

            migrationBuilder.InsertData(
                table: "WIS_CONFIGURATION",
                columns: new[] { "Id", "KEY", "VALUE" },
                values: new object[,]
                {
                    { 1, "MailFrom", "" },
                    { 2, "MailPassword", "" },
                    { 3, "MailSubject", "" },
                    { 4, "MailBody", "" }
                });

            migrationBuilder.InsertData(
                table: "WIS_USER",
                columns: new[] { "Id", "ACTIVATED_DATE", "EMAIL", "FULL_NAME", "NICK_NAME", "PASSWORD", "ROL_ID", "SEND_MAIL", "USER_ACTIVATES" },
                values: new object[] { 1, new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@admin.com", "administrador", "admin", "0LUgYoMMkrK9hwpKJIMogA==", 1, false, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ROL",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
