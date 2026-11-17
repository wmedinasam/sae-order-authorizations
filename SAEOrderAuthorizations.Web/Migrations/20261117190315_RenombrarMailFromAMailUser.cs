using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarMailFromAMailUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conserva el valor de MailFrom. Si ya existe MailUser, no se toca.
            migrationBuilder.Sql("""
                UPDATE WIS_CONFIGURATION SET [KEY] = 'MailUser'
                WHERE [KEY] = 'MailFrom' AND NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailUser');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE WIS_CONFIGURATION SET [KEY] = 'MailFrom'
                WHERE [KEY] = 'MailUser' AND NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailFrom');
                """);
        }
    }
}
