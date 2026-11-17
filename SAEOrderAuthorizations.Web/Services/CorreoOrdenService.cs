using System.Net;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

// Avisa por correo el resultado de autorizar o rechazar una orden. Nunca lanza: un fallo de correo no revierte la orden.
public class CorreoOrdenService(IDbContextFactory<AppDbContext> dbFactory, ILogger<CorreoOrdenService> logger)
{
    public static readonly string[] Marcadores =
        ["{OrdenCompra.IdOrder}", "{OrdenCompra.Status}", "{Empresa.Nombre}", "{Usuario.Nombre}", "{BitacoraEntrada.Descr}"];

    // Reemplaza los marcadores en texto plano. La plantilla no es HTML: el HTML lo genera EnviarAsync.
    public static string Reemplazar(string plantilla, IReadOnlyDictionary<string, string> valores)
    {
        foreach (var (marcador, valor) in valores)
            plantilla = plantilla.Replace(marcador, valor);
        return plantilla;
    }

    public static IReadOnlyDictionary<string, string> Valores(string idOrden, bool aprobada, string empresa, string usuario, string? comentario) =>
        new Dictionary<string, string>
        {
            ["{OrdenCompra.IdOrder}"] = idOrden,
            ["{OrdenCompra.Status}"] = aprobada ? "Aprobada" : "Rechazada",
            ["{Empresa.Nombre}"] = empresa,
            ["{Usuario.Nombre}"] = usuario,
            ["{BitacoraEntrada.Descr}"] = string.IsNullOrWhiteSpace(comentario) ? "Sin observaciones" : comentario.Trim(),
        };

    // Solo para la vista previa de la plantilla en Configuración.
    public static IReadOnlyDictionary<string, string> ValoresEjemplo() =>
        Valores("OC-0001", true, "Empresa de ejemplo", "Usuario de ejemplo", "Autorizada para compra urgente.");

    // Devuelve null si se envió; si no, el motivo (se registra y, en la prueba, se muestra al admin).
    // El correo sale como "nombreUsuario <cuenta SMTP>" porque Gmail/Outlook reescriben el De a la cuenta
    // que autentica; las respuestas van al emailUsuario (Reply-To).
    public async Task<string?> EnviarResultadoAsync(Empresa empresa, OrdenCompra orden, string nombreUsuario, string? emailUsuario, bool aprobada, string? comentario)
    {
        var estado = aprobada ? "Aprobada" : "Rechazada";
        var valores = Valores(orden.IdOrder, aprobada, empresa.Nombre ?? string.Empty, nombreUsuario, comentario);

        var error = await EnviarAsync($"Orden {orden.IdOrder} {estado} · {empresa.Nombre}", valores, nombreUsuario, emailUsuario);
        if (error is not null)
            logger.LogWarning("No se envió el correo de la orden {IdOrder}: {Motivo}", orden.IdOrder, error);
        return error;
    }

    private async Task<string?> EnviarAsync(string asunto, IReadOnlyDictionary<string, string> valores, string nombreUsuario, string? emailUsuario)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var cfg = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var fila in await db.Configuraciones.Where(c => c.Estado).ToListAsync())
            if (fila.Key is not null) cfg[fila.Key] = fila.Value ?? string.Empty;

        var host = cfg.GetValueOrDefault("MailHost", string.Empty).Trim();
        var cuentaSmtp = cfg.GetValueOrDefault("MailUser", string.Empty).Trim();
        var password = cfg.GetValueOrDefault("MailPassword", string.Empty);
        var destino = cfg.GetValueOrDefault("MailTo", string.Empty).Trim();
        var copias = cfg.GetValueOrDefault("MailToCC", string.Empty);
        var plantilla = cfg.GetValueOrDefault("MailBody", string.Empty);

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(cuentaSmtp) || string.IsNullOrEmpty(password)
            || string.IsNullOrEmpty(destino) || string.IsNullOrEmpty(plantilla))
            return "Falta configuración de correo (servidor, remitente, contraseña, destinatario o plantilla).";

        if (!int.TryParse(cfg.GetValueOrDefault("MailPort"), out var puerto) || puerto is < 1 or > 65535)
            return "MailPort inválido.";

        if (!MailboxAddress.TryParse(cuentaSmtp, out var cuenta) || !MailboxAddress.TryParse(destino, out var para))
            return "MailUser o MailTo no es una dirección válida.";

        var direcciones = copias.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (!direcciones.All(d => MailboxAddress.TryParse(d, out _)))
            return "MailToCC contiene una dirección no válida.";

        var texto = Reemplazar(plantilla, valores);
        // Un solo HtmlEncode sobre el texto final: los valores (comentario, nombres) no pueden inyectar HTML.
        var html = WebUtility.HtmlEncode(texto).Replace("\n", "<br>");

        var mensaje = new MimeMessage { Subject = asunto };
        mensaje.From.Add(new MailboxAddress(nombreUsuario, cuenta.Address));
        // Sin Reply-To si el usuario no tiene correo válido: no debe impedir el envío.
        if (!string.IsNullOrWhiteSpace(emailUsuario) && MailboxAddress.TryParse(emailUsuario.Trim(), out var respuesta))
            mensaje.ReplyTo.Add(respuesta);
        mensaje.To.Add(para);
        foreach (var cc in direcciones) mensaje.Cc.Add(MailboxAddress.Parse(cc));
        mensaje.Body = new BodyBuilder { TextBody = texto, HtmlBody = html }.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient { Timeout = 15_000 };
            // Puerto 465: SSL implícito. Cualquier otro (587 en Gmail y Outlook): STARTTLS.
            var seguridad = puerto == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await smtp.ConnectAsync(host, puerto, seguridad);
            await smtp.AuthenticateAsync(cuenta.Address, password);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error SMTP al enviar correo.");
            return ex.Message;
        }
    }
}
