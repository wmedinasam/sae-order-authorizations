using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

// Valor es null en claves sensibles: la UI nunca recibe la contraseña del correo.
public record ConfiguracionItem(int Id, string Key, string? Valor, bool Estado, bool Sensible);

public class ConfiguracionService(AppDbContext db)
{
    // Claves que lee el correo (MailService legacy). No se pueden desactivar ni borrar.
    public static readonly string[] ClavesBase = ["MailFrom", "MailPassword", "MailSubject", "MailBody"];

    private const string ClaveSensible = "MailPassword";
    private static readonly Regex SinEspacios = new(@"^\S+$", RegexOptions.Compiled);
    private static readonly ResultadoOperacion Denegado = new(false, "Solo un administrador puede modificar la configuración.");

    public async Task<List<ConfiguracionItem>> ListarAsync(ClaimsPrincipal usuario, bool incluirInactivas)
    {
        if (!usuario.EsAdministrador()) return new();

        var filas = await db.Configuraciones
            .Where(c => incluirInactivas || c.Estado)
            .OrderBy(c => c.Key)
            .ToListAsync();

        return filas
            .Select(c => new ConfiguracionItem(c.Id, c.Key ?? string.Empty,
                EsSensible(c.Key) ? null : c.Value, c.Estado, EsSensible(c.Key)))
            .ToList();
    }

    public async Task<ResultadoOperacion> CrearAsync(ClaimsPrincipal usuario, string? key, string? valor)
    {
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
        if (!usuario.EsAdministrador()) return Denegado;

        var c = await db.Configuraciones.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return new(false, "La configuración no existe.");

        var sensible = EsSensible(c.Key);
        if (!(sensible && string.IsNullOrEmpty(valor)))
        {
            var nuevo = sensible ? valor! : (valor?.Trim() ?? string.Empty);
            var errorValor = ValidarValor(nuevo);
            if (errorValor is not null) return new(false, errorValor);
            c.Value = nuevo;
        }

        if (!activo && EsBase(c.Key)) return new(false, "Las claves base del correo no se pueden desactivar.");
        c.Estado = activo;

        await db.SaveChangesAsync();
        return new(true, "Configuración guardada.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(ClaimsPrincipal usuario, int id, bool activo)
    {
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
        (valor?.Length ?? 0) > 255 ? "El valor no puede tener más de 255 caracteres." : null;
}
