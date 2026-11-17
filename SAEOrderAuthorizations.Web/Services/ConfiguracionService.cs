using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

// Valor es null en claves sensibles: la UI nunca recibe la contraseña del correo.
public record ConfiguracionItem(int Id, string Key, string? Valor, bool Estado, bool Sensible);

// Formulario de la sección Correo. Es clase (no record) porque Razor necesita @bind sobre sus propiedades.
public class CorreoConfigItem
{
    public string Host { get; set; } = string.Empty;
    public string Puerto { get; set; } = string.Empty;
    // Cuenta SMTP que autentica el envío (MailUser). El remitente visible es el usuario que procesa la orden.
    public string UsuarioSmtp { get; set; } = string.Empty;
    public string? NuevaPassword { get; set; }
    public bool TienePassword { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    // Varias direcciones separadas por coma o punto y coma.
    public string CopiaCC { get; set; } = string.Empty;
    public string Plantilla { get; set; } = string.Empty;
}

public class ConfiguracionService(IDbContextFactory<AppDbContext> dbFactory)
{
    // Claves del correo: se editan en la sección Correo, no en la tabla. No se pueden desactivar ni borrar.
    // MailSubject ya no se usa en el envío (el destinatario es MailTo); se conserva oculta para no dejar huérfana la fila.
    public static readonly string[] ClavesBase = ["MailHost", "MailPort", "MailUser", "MailPassword", "MailSubject", "MailTo", "MailToCC", "MailBody"];

    private const string ClaveSensible = "MailPassword";
    private static readonly Regex SinEspacios = new(@"^\S+$", RegexOptions.Compiled);
    private static readonly ResultadoOperacion Denegado = new(false, "Solo un administrador puede modificar la configuración.");

    public async Task<List<ConfiguracionItem>> ListarAsync(ClaimsPrincipal usuario, bool incluirInactivas)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return new();

        var filas = await db.Configuraciones
            .Where(c => incluirInactivas || c.Estado)
            .OrderBy(c => c.Key)
            .ToListAsync();

        return filas
            .Where(c => !EsBase(c.Key))
            .Select(c => new ConfiguracionItem(c.Id, c.Key ?? string.Empty,
                EsSensible(c.Key) ? null : c.Value, c.Estado, EsSensible(c.Key)))
            .ToList();
    }

    public async Task<CorreoConfigItem?> ObtenerCorreoAsync(ClaimsPrincipal usuario)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return null;

        var filas = await db.Configuraciones.ToListAsync();
        string Valor(string clave) => filas.FirstOrDefault(c => string.Equals(c.Key, clave, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        return new CorreoConfigItem
        {
            Host = Valor("MailHost"),
            Puerto = Valor("MailPort"),
            UsuarioSmtp = Valor("MailUser"),
            TienePassword = Valor("MailPassword").Length > 0,
            Destinatario = Valor("MailTo"),
            CopiaCC = Valor("MailToCC"),
            Plantilla = Valor("MailBody"),
        };
    }

    // NuevaPassword en blanco = conservar la actual. La contraseña nunca regresa a la UI.
    public async Task<ResultadoOperacion> GuardarCorreoAsync(ClaimsPrincipal usuario, CorreoConfigItem datos)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;

        var host = datos.Host.Trim();
        var puerto = datos.Puerto.Trim();
        var usuarioSmtp = datos.UsuarioSmtp.Trim();
        var destinatario = datos.Destinatario.Trim();
        var plantilla = datos.Plantilla.Trim();
        var nuevaClave = datos.NuevaPassword ?? string.Empty;

        if (host.Length == 0 || host.Length > 255 || !SinEspacios.IsMatch(host))
            return new(false, "El servidor SMTP no es válido.");
        if (!int.TryParse(puerto, out var numeroPuerto) || numeroPuerto is < 1 or > 65535)
            return new(false, "El puerto debe estar entre 1 y 65535.");
        if (!MailboxAddress.TryParse(usuarioSmtp, out _))
            return new(false, "La cuenta SMTP (MailUser) no es válida.");
        if (!MailboxAddress.TryParse(destinatario, out _))
            return new(false, "El correo destinatario (MailTo) no es válido.");
        if (ParsearCopias(datos.CopiaCC) is not { } copias)
            return new(false, "Una dirección en copia (MailToCC) no es válida. Sepáralas con coma o punto y coma.");
        if (plantilla.Length == 0)
            return new(false, "La plantilla no puede estar vacía.");
        if (ValidarValor(plantilla) is { } errorPlantilla)
            return new(false, errorPlantilla);
        if (ValidarValor(nuevaClave) is { } errorClave)
            return new(false, errorClave);

        var desconocido = Regex.Matches(plantilla, @"\{[^{}]*\}")
            .Select(m => m.Value)
            .FirstOrDefault(v => !CorreoOrdenService.Marcadores.Contains(v));
        if (desconocido is not null)
            return new(false, $"Marcador no válido: {desconocido}. Usa solo los botones de marcadores.");

        await GuardarValorAsync(db, "MailHost", host);
        await GuardarValorAsync(db, "MailPort", puerto);
        await GuardarValorAsync(db, "MailUser", usuarioSmtp);
        if (nuevaClave.Length > 0) await GuardarValorAsync(db, "MailPassword", nuevaClave);
        await GuardarValorAsync(db, "MailTo", destinatario);
        await GuardarValorAsync(db, "MailToCC", string.Join(", ", copias));
        await GuardarValorAsync(db, "MailBody", plantilla);

        await db.SaveChangesAsync();
        return new(true, "Configuración de correo guardada.");
    }

    // Acepta coma o punto y coma (según el teclado o el SO). Null si alguna dirección no es válida.
    private static List<string>? ParsearCopias(string texto)
    {
        var direcciones = texto.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return direcciones.All(d => MailboxAddress.TryParse(d, out _)) ? direcciones.ToList() : null;
    }

    // Las claves base siempre quedan activas: si una estaba inactiva, se reactiva al guardar.
    private async Task GuardarValorAsync(AppDbContext db, string clave, string valor)
    {
        var fila = await db.Configuraciones.FirstOrDefaultAsync(c => c.Key == clave);
        if (fila is null)
            db.Configuraciones.Add(new Configuracion { Key = clave, Value = valor, Estado = true });
        else
        {
            fila.Value = valor;
            fila.Estado = true;
        }
    }

    public async Task<ResultadoOperacion> CrearAsync(ClaimsPrincipal usuario, string? key, string? valor)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;

        key = key?.Trim() ?? string.Empty;
        var error = ValidarClave(key) ?? ValidarValor(valor);
        if (error is not null) return new(false, error);

        // Única sin distinguir mayúsculas, incluidas las inactivas (reactivar no debe chocar).
        var minusculas = key.ToLowerInvariant();
        if (await db.Configuraciones.AnyAsync(c => c.Key != null && c.Key.ToLower() == minusculas))
            return new(false, "Ya existe una configuración con esa clave.");

        db.Configuraciones.Add(new Configuracion { Key = key, Value = valor ?? string.Empty, Estado = true });
        await db.SaveChangesAsync();
        return new(true, "Configuración creada.");
    }

    // La clave no se cambia. En claves sensibles, valor en blanco = conservar el actual.
    public async Task<ResultadoOperacion> EditarAsync(ClaimsPrincipal usuario, int id, string? valor, bool activo)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;

        var c = await db.Configuraciones.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return new(false, "La configuración no existe.");

        // Se valida todo antes de tocar la entidad: un error no debe dejar cambios rastreados.
        if (!activo && EsBase(c.Key)) return new(false, "Las claves base del correo no se pueden desactivar.");

        var sensible = EsSensible(c.Key);
        if (!(sensible && string.IsNullOrEmpty(valor)))
        {
            var nuevo = sensible ? valor! : (valor?.Trim() ?? string.Empty);
            var errorValor = ValidarValor(nuevo);
            if (errorValor is not null) return new(false, errorValor);
            c.Value = nuevo;
        }

        c.Estado = activo;

        await db.SaveChangesAsync();
        return new(true, "Configuración guardada.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(ClaimsPrincipal usuario, int id, bool activo)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;

        var c = await db.Configuraciones.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return new(false, "La configuración no existe.");
        if (!activo && EsBase(c.Key)) return new(false, "Las claves base del correo no se pueden desactivar.");

        c.Estado = activo;
        await db.SaveChangesAsync();
        return new(true, activo ? "Configuración reactivada." : "Configuración desactivada.");
    }

    private static bool EsSensible(string? key) => string.Equals(key, ClaveSensible, StringComparison.OrdinalIgnoreCase);

    private static bool EsBase(string? key) => ClavesBase.Contains(key, StringComparer.OrdinalIgnoreCase);

    private static string? ValidarClave(string key)
    {
        if (key.Length == 0) return "La clave es obligatoria.";
        if (key.Length > 25) return "La clave no puede tener más de 25 caracteres.";
        if (!SinEspacios.IsMatch(key)) return "La clave no puede contener espacios.";
        return null;
    }

    private static string? ValidarValor(string? valor) =>
        (valor?.Length ?? 0) > 4000 ? "El valor no puede tener más de 4000 caracteres." : null;
}
