using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoUsuarioYConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ESTADO",
                table: "WIS_USER",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ESTADO",
                table: "WIS_CONFIGURATION",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 1,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 2,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 3,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_CONFIGURATION",
                keyColumn: "Id",
                keyValue: 4,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 1,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 2,
                column: "ESTADO",
                value: true);

            migrationBuilder.UpdateData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 3,
                column: "ESTADO",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ESTADO",
                table: "WIS_USER");

            migrationBuilder.DropColumn(
                name: "ESTADO",
                table: "WIS_CONFIGURATION");
        }
    }
}
