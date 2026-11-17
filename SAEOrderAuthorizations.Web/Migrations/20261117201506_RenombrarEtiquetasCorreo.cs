using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAEOrderAuthorizations.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarEtiquetasCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Etiquetas con nombre de negocio. Idempotente: si ya no hay etiquetas antiguas, no cambia nada.
            migrationBuilder.Sql("""
                UPDATE WIS_CONFIGURATION SET [VALUE] = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([VALUE], N'{OrdenCompra.IdOrder}', N'{Número de Orden de Compra}'), N'{Empresa.Nombre}', N'{Empresa}'), N'{OrdenCompra.Status}', N'{Estado de la Orden}'), N'{Usuario.Nombre}', N'{Usuario que Procesó}'), N'{BitacoraEntrada.Descr}', N'{Observación}')
                WHERE [KEY] = 'MailBody';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE WIS_CONFIGURATION SET [VALUE] = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([VALUE], N'{Número de Orden de Compra}', N'{OrdenCompra.IdOrder}'), N'{Empresa}', N'{Empresa.Nombre}'), N'{Estado de la Orden}', N'{OrdenCompra.Status}'), N'{Usuario que Procesó}', N'{Usuario.Nombre}'), N'{Observación}', N'{BitacoraEntrada.Descr}')
                WHERE [KEY] = 'MailBody';
                """);
        }
    }
}
