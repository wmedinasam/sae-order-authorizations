using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class SemillaEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EMPRESA",
                columns: new[] { "Id", "DB_SAE", "Direccion", "Estado", "Nombre", "Sufijo" },
                values: new object[] { 1, "SAE80Empre01", null, true, "Empresa 01", "01" });

            migrationBuilder.InsertData(
                table: "ROL_EMPRESA",
                columns: new[] { "EmpresaId", "RolId" },
                values: new object[] { 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EMPRESA",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ROL_EMPRESA",
                keyColumns: new[] { "EmpresaId", "RolId" },
                keyValues: new object[] { 1, 1 });
        }
    }
}
