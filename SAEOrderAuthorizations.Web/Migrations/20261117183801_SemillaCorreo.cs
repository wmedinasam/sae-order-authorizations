using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class SemillaCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: no duplica claves ni pisa una plantilla que el admin ya editó.
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailHost')
                    INSERT INTO WIS_CONFIGURATION ([KEY], [VALUE], ESTADO) VALUES ('MailHost', 'smtp.gmail.com', 1);

                IF NOT EXISTS (SELECT 1 FROM WIS_CONFIGURATION WHERE [KEY] = 'MailPort')
                    INSERT INTO WIS_CONFIGURATION ([KEY], [VALUE], ESTADO) VALUES ('MailPort', '587', 1);

                UPDATE WIS_CONFIGURATION
                SET [VALUE] = N'La Orden de compra con número: {OrdenCompra.IdOrder} de la empresa {Empresa.Nombre} ha sido {OrdenCompra.Status} por {Usuario.Nombre} observacion: {BitacoraEntrada.Descr}'
                WHERE [KEY] = 'MailBody' AND ([VALUE] IS NULL OR [VALUE] = '');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
