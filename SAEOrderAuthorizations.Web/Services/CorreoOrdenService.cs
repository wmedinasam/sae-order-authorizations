using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Ganss.Xss;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public record MarcadorCorreo(string Etiqueta, string Descripcion);

// Datos de la orden que llegan a la plantilla. Cada uno corresponde a una etiqueta visible.
public record DatosCorreo(string NumeroOrden, string Empresa, bool Aprobada, string Usuario, string? Comentario,
    string? Proveedor, double Total, DateTime FechaOrden, DateTime FechaResolucion);

// Avisa por correo el resultado de autorizar o rechazar una orden. Nunca lanza: un fallo de correo no revierte la orden.
public class CorreoOrdenService(IDbContextFactory<AppDbContext> dbFactory, ILogger<CorreoOrdenService> logger)
{
    private const string EtNumero = "{Número de Orden de Compra}"; // OrdenCompra.IdOrder
    private const string EtEmpresa = "{Empresa}";                   // Empresa.Nombre de la empresa dueña de la orden
    private const string EtEstado = "{Estado de la Orden}";         // OrdenCompra.Status: Aprobada / Rechazada
    private const string EtUsuario = "{Usuario que Procesó}";       // Usuario que autorizó o rechazó
    private const string EtObservacion = "{Observación}";           // Comentario de la autorización o rechazo
    private const string EtProveedor = "{Proveedor}";               // Nombre en SAE; si SAE no responde, IdSupplier
    private const string EtTotal = "{Total de la Orden}";           // OrdenCompra.Total
    private const string EtFechaOrden = "{Fecha de la Orden}";      // OrdenCompra.OrdDate
    private const string EtFechaResolucion = "{Fecha de Resolución}"; // Momento del envío

    public static readonly MarcadorCorreo[] Marcadores =
    [
        new(EtNumero, "Número de la orden de compra"),
        new(EtEmpresa, "Empresa dueña de la orden"),
        new(EtEstado, "Aprobada o Rechazada"),
        new(EtUsuario, "Usuario que autorizó o rechazó"),
        new(EtObservacion, "Comentario de la autorización o rechazo"),
        new(EtProveedor, "Nombre del proveedor"),
        new(EtTotal, "Total de la orden"),
        new(EtFechaOrden, "Fecha en que se generó la orden"),
        new(EtFechaResolucion, "Fecha y hora de la autorización o rechazo"),
    ];

    // Plantilla de ejemplo para el botón "Cargar plantilla base". No se guarda hasta pulsar Guardar.
    public const string PlantillaBase = """
        <div style="font-family: Arial, sans-serif; color: #222222;">
          <h2 style="color: #1F3A5F;">Orden de compra {Estado de la Orden}</h2>
          <p>La Orden de compra con número <b>{Número de Orden de Compra}</b> de la empresa <b>{Empresa}</b> ha sido <b>{Estado de la Orden}</b> por {Usuario que Procesó}.</p>
          <table cellpadding="6" cellspacing="0" border="1" style="border-collapse: collapse; border-color: #cccccc;">
            <tr><td><b>Proveedor</b></td><td>{Proveedor}</td></tr>
            <tr><td><b>Total</b></td><td>{Total de la Orden}</td></tr>
            <tr><td><b>Fecha de la orden</b></td><td>{Fecha de la Orden}</td></tr>
            <tr><td><b>Fecha de resolución</b></td><td>{Fecha de Resolución}</td></tr>
            <tr><td><b>Observación</b></td><td>{Observación}</td></tr>
          </table>
        </div>
        """;

    private static readonly HtmlSanitizer Sanitizador = CrearSanitizador();

    public static IReadOnlyDictionary<string, string> Valores(DatosCorreo d) => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [EtNumero] = d.NumeroOrden,
        [EtEmpresa] = d.Empresa,
        [EtEstado] = d.Aprobada ? "Aprobada" : "Rechazada",
        [EtUsuario] = d.Usuario,
        [EtObservacion] = string.IsNullOrWhiteSpace(d.Comentario) ? "Sin observaciones" : d.Comentario.Trim(),
        [EtProveedor] = d.Proveedor?.Trim() ?? string.Empty,
        [EtTotal] = d.Total.ToString("N2", CultureInfo.InvariantCulture),
        [EtFechaOrden] = d.FechaOrden.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        [EtFechaResolucion] = d.FechaResolucion.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture),
    };

    // Solo para la vista previa de la plantilla en Configuración.
    public static IReadOnlyDictionary<string, string> ValoresEjemplo() => Valores(new DatosCorreo(
        "OC-0001", "Empresa de ejemplo", true, "Usuario de ejemplo", "Autorizada para compra urgente.",
        "Proveedor de ejemplo", 12500.5, new DateTime(2026, 11, 17), DateTime.Now));

    // Una sola pasada: un valor insertado nunca se vuelve a examinar como etiqueta.
    public static string Reemplazar(string plantilla, IReadOnlyDictionary<string, string> valores) =>
        Regex.Replace(plantilla, @"\{[^{}]+\}", m => valores.TryGetValue(m.Value, out var valor) ? valor : m.Value);

    // HTML del correo y de la vista previa. Los valores van codificados: un comentario no puede inyectar HTML.
    // Una plantilla sin etiquetas HTML (texto plano heredado) se codifica completa, como antes.
    public static string ArmarHtml(string plantilla, IReadOnlyDictionary<string, string> valores)
    {
        if (!Regex.IsMatch(plantilla, "<[A-Za-z!/]"))
            return WebUtility.HtmlEncode(Reemplazar(plantilla, valores)).Replace("\n", "<br>");

        var codificados = valores.ToDictionary(
            v => v.Key,
            v => WebUtility.HtmlEncode(v.Value).Replace("\n", "<br>"),
            StringComparer.OrdinalIgnoreCase);
        return Sanitizador.Sanitize(Reemplazar(plantilla, codificados));
    }

    // Versión de texto del correo, derivada del HTML ya saneado.
    public static string ArmarTexto(string html)
    {
        var conSaltos = Regex.Replace(html, @"<br\s*/?>|</(p|div|tr|li|h[1-6])>", "\n", RegexOptions.IgnoreCase);
        return WebUtility.HtmlDecode(Regex.Replace(conSaltos, "<[^>]+>", string.Empty)).Trim();
    }

    // Devuelve null si se envió; si no, el motivo (se registra en el log).
    // El correo sale como "usuario <cuenta SMTP>": Gmail/Outlook reescriben el De a la cuenta que autentica.
    // Las respuestas van al emailUsuario (Reply-To).
    public async Task<string?> EnviarResultadoAsync(OrdenCompra orden, string nombreUsuario, string? emailUsuario, bool aprobada, string? comentario, string? proveedor)
    {
        var empresa = await NombreEmpresaAsync(orden.EmpresaId, orden.IdOrder);
        var datos = new DatosCorreo(orden.IdOrder, empresa, aprobada, nombreUsuario, comentario,
            proveedor ?? orden.IdSupplier, orden.Total, orden.OrdDate, DateTime.Now);
        var estado = aprobada ? "Aprobada" : "Rechazada";

        var error = await EnviarAsync($"Orden {orden.IdOrder} {estado} · {empresa}", datos, emailUsuario);
        if (error is not null)
            logger.LogWarning("No se envió el correo de la orden {IdOrder}: {Motivo}", orden.IdOrder, error);
        return error;
    }

    // La empresa sale de la orden, no de la sesión: en un entorno multiempresa cada orden tiene la suya.
    private async Task<string> NombreEmpresaAsync(int? empresaId, string idOrden)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var nombre = await db.Empresas.Where(e => e.Id == empresaId).Select(e => e.Nombre).FirstOrDefaultAsync();
        if (nombre is null) logger.LogWarning("La orden {IdOrder} no tiene una empresa válida.", idOrden);
        return nombre ?? string.Empty;
    }

    private async Task<string?> EnviarAsync(string asunto, DatosCorreo datos, string? emailUsuario)
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

        var html = ArmarHtml(plantilla, Valores(datos));

        var mensaje = new MimeMessage { Subject = asunto };
        mensaje.From.Add(new MailboxAddress(datos.Usuario, cuenta.Address));
        // Sin Reply-To si el usuario no tiene correo válido: no debe impedir el envío.
        if (!string.IsNullOrWhiteSpace(emailUsuario) && MailboxAddress.TryParse(emailUsuario.Trim(), out var respuesta))
            mensaje.ReplyTo.Add(respuesta);
        mensaje.To.Add(para);
        foreach (var cc in direcciones) mensaje.Cc.Add(MailboxAddress.Parse(cc));
        mensaje.Body = new BodyBuilder { TextBody = ArmarTexto(html), HtmlBody = html }.ToMessageBody();

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

    // Lista blanca: solo etiquetas y atributos de maquetado, y enlaces o imágenes http(s)/mailto.
    private static HtmlSanitizer CrearSanitizador()
    {
        var s = new HtmlSanitizer();
        s.AllowedTags.Clear();
        foreach (var t in new[] { "p", "div", "span", "br", "hr", "b", "strong", "i", "em", "u", "small",
            "h1", "h2", "h3", "h4", "h5", "h6", "ul", "ol", "li", "table", "thead", "tbody", "tr", "td", "th",
            "a", "img", "font", "center", "blockquote" })
            s.AllowedTags.Add(t);
        s.AllowedAttributes.Clear();
        foreach (var a in new[] { "style", "class", "href", "src", "alt", "title", "width", "height", "align", "valign",
            "colspan", "rowspan", "bgcolor", "border", "cellpadding", "cellspacing", "color", "target" })
            s.AllowedAttributes.Add(a);
        s.AllowedSchemes.Clear();
        foreach (var e in new[] { "http", "https", "mailto" }) s.AllowedSchemes.Add(e);
        return s;
    }
}
