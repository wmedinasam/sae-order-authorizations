using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public enum EstadoLicencia { SinLicencia, Invalida, Vigente, EnGracia, Vencida, VersionNoCubierta, RelojAlterado }

public enum LimiteLicencia { Empresas, Usuarios }

// Datos: última licencia verificada (null si no hay o no verifica). DiasRestantes: null en lifetime.
public record EstadoLicenciaInfo(EstadoLicencia Estado, string Mensaje, LicenciaPayload? Datos, int? DiasRestantes)
{
    public bool PermiteEscritura => Estado is EstadoLicencia.Vigente or EstadoLicencia.EnGracia;
}

public class LicenciaService(IDbContextFactory<AppDbContext> dbFactory, IMemoryCache cache, IConfiguration config)
{
    private const string CacheKey = "licencia:estado";
    private static readonly TimeSpan TiempoCache = TimeSpan.FromMinutes(1);
    // Margen para ajustes legítimos de hora (NTP, zona horaria): solo cuenta como manipulación un retroceso mayor a un día.
    private static readonly TimeSpan MargenReloj = TimeSpan.FromDays(1);
    private const int MaxBytes = 16 * 1024;
    private static readonly ResultadoOperacion Denegado = new(false, "Solo un administrador puede cargar la licencia.");

    public Task<EstadoLicenciaInfo> ObtenerEstadoAsync() =>
        cache.GetOrCreateAsync(CacheKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = TiempoCache;
            return EvaluarAsync();
        })!;

    // Null si la escritura puede continuar; si no, el mensaje que se muestra al usuario.
    public async Task<string?> MotivoBloqueoEscrituraAsync()
    {
        var estado = await ObtenerEstadoAsync();
        return estado.PermiteEscritura ? null : $"Modo solo lectura. {estado.Mensaje}";
    }

    // Se consulta al crear o reactivar; cuenta solo las activas, como exige la edición.
    public async Task<string?> MotivoLimiteAsync(LimiteLicencia limite)
    {
        var datos = (await ObtenerEstadoAsync()).Datos;
        if (datos is null) return "No hay una licencia válida.";

        await using var db = await dbFactory.CreateDbContextAsync();
        if (limite == LimiteLicencia.Empresas)
        {
            var activas = await db.Empresas.CountAsync(e => e.Estado == true);
            return activas >= datos.MaxEmpresas ? $"La licencia permite {datos.MaxEmpresas} empresas activas." : null;
        }

        var usuarios = await db.Usuarios.CountAsync(u => u.Estado == true);
        return usuarios >= datos.MaxUsuarios ? $"La licencia permite {datos.MaxUsuarios} usuarios activos." : null;
    }

    public async Task<Guid?> ObtenerInstalacionAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Instalaciones.AsNoTracking().Select(i => (Guid?)i.InstallationId).FirstOrDefaultAsync();
    }

    // Se llama al arrancar, antes de atender peticiones: así nunca hay dos identificadores en paralelo.
    public async Task AsegurarInstalacionAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (await db.Instalaciones.AnyAsync()) return;

        db.Instalaciones.Add(new InstalacionLicencia { InstallationId = Guid.NewGuid() });
        await db.SaveChangesAsync();
    }

    public async Task<ResultadoOperacion> CargarAsync(ClaimsPrincipal usuario, Stream contenido)
    {
        if (!usuario.EsAdministrador()) return Denegado;

        var bytes = await LeerAsync(contenido);
        if (bytes is null) return new(false, $"El archivo supera el máximo de {MaxBytes / 1024} KB.");

        await using var db = await dbFactory.CreateDbContextAsync();
        var instalacion = await db.Instalaciones.AsNoTracking().Select(i => (Guid?)i.InstallationId).FirstOrDefaultAsync();

        LicenciaPayload datos;
        try
        {
            datos = Abrir(bytes, instalacion);
        }
        catch (LicenciaInvalidaException ex)
        {
            return new(false, ex.Message);
        }

        var ahora = DateTimeOffset.UtcNow;
        if (datos.MaxVersionMayor is { } tope && VersionMayorActual > tope)
            return new(false, $"La licencia cubre hasta la versión {tope}.x y esta aplicación es la {VersionMayorActual}.x.");
        if (ahora < datos.ValidFrom)
            return new(false, $"La licencia inicia el {Fecha(datos.ValidFrom)}.");
        if (datos.ValidTo is { } fin && ahora > fin.AddDays(datos.GraceDays))
            return new(false, $"La licencia venció el {Fecha(fin)}. Pide una renovación.");

        // Impide volver a una licencia anterior: una emisión más vieja o el mismo serial no sustituyen a la instalada.
        var anterior = await db.Licencias.AsNoTracking().OrderByDescending(l => l.Id).FirstOrDefaultAsync();
        if (anterior is not null)
        {
            if (datos.Emitida.UtcDateTime < anterior.Emitida)
                return new(false, "La licencia es más antigua que la que ya está instalada.");
            if (anterior.LicenseId == datos.LicenseId && datos.Serial <= anterior.Serial)
                return new(false, "Ya está instalada esta licencia o una versión más reciente.");
        }

        db.Licencias.Add(new Licencia
        {
            LicenseId = datos.LicenseId,
            Serial = datos.Serial,
            Cliente = datos.Cliente.Trim(),
            Plan = datos.Plan,
            Edicion = datos.Edicion,
            MaxEmpresas = datos.MaxEmpresas,
            MaxUsuarios = datos.MaxUsuarios,
            Emitida = datos.Emitida.UtcDateTime,
            ValidFrom = datos.ValidFrom.UtcDateTime,
            ValidTo = datos.ValidTo?.UtcDateTime,
            GraceDays = datos.GraceDays,
            MaxVersionMayor = datos.MaxVersionMayor,
            Archivo = bytes,
            CargadaUtc = DateTime.UtcNow,
            CargadaPor = usuario.IdUsuario()
        });
        await db.SaveChangesAsync();
        cache.Remove(CacheKey);
        return new(true, "Licencia cargada.");
    }

    private async Task<EstadoLicenciaInfo> EvaluarAsync()
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var fila = await db.Licencias.AsNoTracking().OrderByDescending(l => l.Id).FirstOrDefaultAsync();
        if (fila is null)
            return new(EstadoLicencia.SinLicencia, "No hay una licencia cargada.", null, null);

        var instalacion = await db.Instalaciones.FirstOrDefaultAsync();
        LicenciaPayload datos;
        try
        {
            datos = Abrir(fila.Archivo, instalacion?.InstallationId);
        }
        catch (LicenciaInvalidaException ex)
        {
            return new(EstadoLicencia.Invalida, ex.Message, null, null);
        }

        var ahora = DateTimeOffset.UtcNow;
        if (instalacion!.UltimoVisto is { } visto && ahora < new DateTimeOffset(visto, TimeSpan.Zero) - MargenReloj)
            return new(EstadoLicencia.RelojAlterado,
                "La fecha del servidor es anterior a la última verificación. Corrige la hora del equipo.", datos, null);

        instalacion.UltimoVisto = ahora.UtcDateTime;
        await db.SaveChangesAsync();

        return EvaluarVigencia(datos, ahora, VersionMayorActual);
    }

    // Función pura (sin BD ni reloj) para probar cada estado con fechas fijas.
    public static EstadoLicenciaInfo EvaluarVigencia(LicenciaPayload datos, DateTimeOffset ahora, int versionActual)
    {
        if (datos.MaxVersionMayor is { } tope && versionActual > tope)
            return new(EstadoLicencia.VersionNoCubierta,
                $"La licencia cubre hasta la versión {tope}.x y esta aplicación es la {versionActual}.x.", datos, null);
        if (ahora < datos.ValidFrom)
            return new(EstadoLicencia.Invalida, $"La licencia inicia el {Fecha(datos.ValidFrom)}.", datos, null);

        if (datos.ValidTo is not { } fin)
            return new(EstadoLicencia.Vigente, $"Licencia {datos.Plan} sin vencimiento.", datos, null);

        if (ahora <= fin)
            return new(EstadoLicencia.Vigente, $"Licencia {datos.Plan} vigente hasta el {Fecha(fin)}.", datos, Dias(fin - ahora));

        var finGracia = fin.AddDays(datos.GraceDays);
        if (ahora <= finGracia)
            return new(EstadoLicencia.EnGracia,
                $"Licencia vencida el {Fecha(fin)}. Modo solo lectura en {Dias(finGracia - ahora)} día(s).",
                datos, Dias(finGracia - ahora));

        return new(EstadoLicencia.Vencida, $"Licencia vencida el {Fecha(fin)}.", datos, null);
    }

    // Verifica el archivo con la clave pública y el secreto de la app. Lanza LicenciaInvalidaException.
    private LicenciaPayload Abrir(byte[] archivo, Guid? instalacion)
    {
        if (instalacion is null)
            throw new LicenciaInvalidaException("La instalación no tiene identificador. Reinicia la aplicación.");

        using var clave = ClavePublica()
            ?? throw new LicenciaInvalidaException("La verificación de licencias no está configurada en esta instalación.");
        var secreto = SecretoApp()
            ?? throw new LicenciaInvalidaException("La verificación de licencias no está configurada en esta instalación.");

        return LicenciaFormato.Abrir(archivo, instalacion.Value, clave, secreto);
    }

    private ECDsa? ClavePublica()
    {
        var pem = config["Licencia:ClavePublica"];
        if (string.IsNullOrWhiteSpace(pem)) return null;

        var ecdsa = ECDsa.Create();
        try
        {
            ecdsa.ImportFromPem(pem);
            return ecdsa;
        }
        catch (CryptographicException)
        {
            ecdsa.Dispose();
            throw new LicenciaInvalidaException("La clave pública de licencias no es válida.");
        }
    }

    private byte[]? SecretoApp()
    {
        var valor = config["Licencia:AppSecret"];
        if (string.IsNullOrWhiteSpace(valor)) return null;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(valor);
        }
        catch (FormatException)
        {
            throw new LicenciaInvalidaException("El secreto de licencias no es Base64 válido.");
        }
        return bytes.Length == LicenciaFormato.TamanoSecreto
            ? bytes
            : throw new LicenciaInvalidaException("El secreto de licencias debe tener 32 bytes.");
    }

    private static int VersionMayorActual => typeof(LicenciaService).Assembly.GetName().Version?.Major ?? 1;

    private static int Dias(TimeSpan intervalo) => (int)Math.Ceiling(intervalo.TotalDays);

    private static string Fecha(DateTimeOffset fecha) => fecha.ToLocalTime().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static async Task<byte[]?> LeerAsync(Stream contenido)
    {
        using var memoria = new MemoryStream();
        var buffer = new byte[8192];
        int leido;
        while ((leido = await contenido.ReadAsync(buffer)) > 0)
        {
            if (memoria.Length + leido > MaxBytes) return null;
            memoria.Write(buffer, 0, leido);
        }
        return memoria.ToArray();
    }
}
