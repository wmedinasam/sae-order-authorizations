using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminPasswordClaro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 1,
                column: "PASSWORD",
                value: "123");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WIS_USER",
                keyColumn: "Id",
                keyValue: 1,
                column: "PASSWORD",
                value: "0LUgYoMMkrK9hwpKJIMogA==");
        }
    }
}
