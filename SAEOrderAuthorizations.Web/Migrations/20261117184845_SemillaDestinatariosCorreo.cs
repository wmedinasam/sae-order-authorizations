using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class SemillaDestinatariosCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: vacías para que el admin las configure en la sección Correo.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailTo')
                    INSERT INTO WIS_CONFIGURATION ([KEY], [VALUE], ESTADO) VALUES ('MailTo', '', 1);

                IF NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailToCC')
                    INSERT INTO WIS_CONFIGURATION ([KEY], [VALUE], ESTADO) VALUES ('MailToCC', '', 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
