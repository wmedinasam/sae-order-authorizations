using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public record MarcaInfo(string LogoSidebarUrl, string LogoLoginUrl, string FaviconUrl, string FaviconTipo, string NombreEmpresa);

public class MarcaService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache)
{
    public static readonly string[] Slots = ["LogoSidebar", "LogoLogin", "Favicon"];

    private const string CacheKey = "marca:info";
    private const string ClaveNombre = "CompanyName";
    private const string NombreDefecto = "WIS Dev Code";
    private const string LogoDefecto = "brand/logos/svg/wis-logo-horizontal-on-dark.svg";
    private const string FaviconDefecto = "brand/web-icons/favicon.svg";
    private const int MaxLogoBytes = 512 * 1024;
    private const int MaxFaviconBytes = 128 * 1024;
    private const int MaxNombre = 255;

    private static readonly byte[] FirmaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] FirmaJpg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] FirmaIco = [0x00, 0x00, 0x01, 0x00];

    private static readonly ResultadoOperacion Denegado = new(false, "Solo un administrador puede modificar la marca.");

    // Las páginas y layouts piden la marca a la vez y comparten el mismo DbContext del request:
    // se reutiliza una sola tarea por scope para no lanzar dos consultas concurrentes.
    private Task<MarcaInfo>? _enCurso;

    public Task<MarcaInfo> ObtenerAsync() => _enCurso ??= cache.GetOrCreateAsync(CacheKey, _ => CargarAsync())!;

    private void Invalidar()
    {
        cache.Remove(CacheKey);
        _enCurso = null;
    }

    public async Task<ResultadoOperacion> GuardarAsync(ClaimsPrincipal usuario, string slot, Stream contenido, string nombreArchivo)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;
        if (!Slots.Contains(slot)) return new(false, "El elemento de marca no es válido.");

        var favicon = slot == "Favicon";
        var limite = favicon ? MaxFaviconBytes : MaxLogoBytes;
        var bytes = await LeerAsync(contenido, limite);
        if (bytes is null) return new(false, $"El archivo supera el máximo de {limite / 1024} KB.");

        // Se valida por firma del contenido; la extensión y el tipo declarado por el navegador no son confiables.
        var tipo = DetectarTipo(bytes, favicon);
        if (tipo is null)
            return new(false, favicon
                ? "Formato no válido. Usa PNG, JPG, WebP o ICO."
                : "Formato no válido. Usa PNG, JPG o WebP.");

        var fila = await db.Marcas.FirstOrDefaultAsync(m => m.Slot == slot);
        if (fila is null)
        {
            fila = new Marca { Slot = slot };
            db.Marcas.Add(fila);
        }

        fila.Contenido = bytes;
        fila.TipoMime = tipo;
        fila.NombreArchivo = nombreArchivo.Length > MaxNombre ? nombreArchivo[..MaxNombre] : nombreArchivo;
        fila.Actualizado = DateTime.UtcNow;
        fila.Estado = true;

        await db.SaveChangesAsync();
        Invalidar();
        return new(true, "Imagen guardada.");
    }

    public async Task<ResultadoOperacion> RestaurarAsync(ClaimsPrincipal usuario, string slot)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;
        if (!Slots.Contains(slot)) return new(false, "El elemento de marca no es válido.");

        var fila = await db.Marcas.FirstOrDefaultAsync(m => m.Slot == slot);
        if (fila is null || !fila.Estado) return new(false, "Este elemento ya usa el valor por defecto.");

        // Soft delete: la fila queda para auditoría y la siguiente subida la reactiva.
        fila.Estado = false;
        await db.SaveChangesAsync();
        Invalidar();
        return new(true, "Imagen restaurada al valor por defecto.");
    }

    public async Task<ResultadoOperacion> GuardarNombreAsync(ClaimsPrincipal usuario, string? nombre)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!usuario.EsAdministrador()) return Denegado;

        var valor = nombre?.Trim() ?? string.Empty;
        if (valor.Length > MaxNombre) return new(false, "El nombre no puede tener más de 255 caracteres.");

        // Busca también inactivas: reutilizar la fila evita duplicar la clave.
        var fila = await db.Configuraciones.FirstOrDefaultAsync(c => c.Key == ClaveNombre);
        if (valor.Length == 0)
        {
            if (fila is not null) fila.Estado = false;
        }
        else if (fila is null)
        {
            db.Configuraciones.Add(new Configuracion { Key = ClaveNombre, Value = valor, Estado = true });
        }
        else
        {
            fila.Value = valor;
            fila.Estado = true;
        }

        await db.SaveChangesAsync();
        Invalidar();
        return new(true, valor.Length == 0 ? "Nombre restaurado a WIS Dev Code." : "Nombre de la empresa guardado.");
    }

    private async Task<MarcaInfo> CargarAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        // Proyección sin Contenido: esta consulta corre en cada cambio de caché, no hay que traer los bytes.
        var activas = await db.Marcas
            .Where(m => m.Estado)
            .Select(m => new { m.Slot, m.TipoMime, m.Actualizado })
            .ToDictionaryAsync(m => m.Slot);

        string? Url(string slot) => activas.TryGetValue(slot, out var f) ? $"/branding/{slot}?v={f.Actualizado.Ticks}" : null;

        var nombre = await db.Configuraciones
            .Where(c => c.Key == ClaveNombre && c.Estado)
            .Select(c => c.Value)
            .FirstOrDefaultAsync();

        var favicon = activas.TryGetValue("Favicon", out var fav) ? fav : null;

        return new MarcaInfo(
            LogoSidebarUrl: Url("LogoSidebar") ?? LogoDefecto,
            LogoLoginUrl: Url("LogoLogin") ?? Url("LogoSidebar") ?? LogoDefecto,
            FaviconUrl: Url("Favicon") ?? FaviconDefecto,
            FaviconTipo: favicon?.TipoMime ?? "image/svg+xml",
            NombreEmpresa: string.IsNullOrWhiteSpace(nombre) ? NombreDefecto : nombre.Trim());
    }

    // Lee como máximo `limite` bytes; null si el archivo es más grande.
    private static async Task<byte[]?> LeerAsync(Stream stream, int limite)
    {
        using var memoria = new MemoryStream();
        var buffer = new byte[16 * 1024];
        int leidos;
        while ((leidos = await stream.ReadAsync(buffer)) > 0)
        {
            if (memoria.Length + leidos > limite) return null;
            memoria.Write(buffer, 0, leidos);
        }
        return memoria.ToArray();
    }

    private static string? DetectarTipo(byte[] bytes, bool favicon)
    {
        if (bytes.AsSpan().StartsWith(FirmaPng)) return "image/png";
        if (bytes.AsSpan().StartsWith(FirmaJpg)) return "image/jpeg";
        if (bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";
        if (favicon && bytes.AsSpan().StartsWith(FirmaIco)) return "image/x-icon";
        return null;
    }
}
