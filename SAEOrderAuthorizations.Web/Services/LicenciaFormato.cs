using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SAEOrderAuthorizations.Web.Services;

// Formato del .lic emitido por wis-license (PLAN-CLI-LICENCIAS.md §4-5). Cambiarlo exige cambiar el emisor.
public static class LicenciaFormato
{
    public const int VersionFormato = 1;
    public const int TamanoSecreto = 32;

    private static readonly byte[] Magic = Encoding.ASCII.GetBytes("WISLIC1");
    private static readonly byte[] InfoClave = Encoding.ASCII.GetBytes("WISLIC1-aes");
    private const int OffsetNonce = 8;
    private const int TamanoNonce = 12;
    private const int OffsetLongitud = 20;
    private const int OffsetCifrado = 24;
    private const int TamanoEncabezado = 20;
    private const int TamanoTag = 16;
    private const int TamanoFirma = 64;

    private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

    // Verifica la firma antes de descifrar. Lanza LicenciaInvalidaException con un mensaje apto para la UI.
    public static LicenciaPayload Abrir(byte[] archivo, Guid installationId, ECDsa clavePublica, byte[] appSecret)
    {
        const string noValida = "El archivo no es una licencia válida.";
        if (archivo.Length < OffsetCifrado + TamanoTag + TamanoFirma
            || !archivo.AsSpan(0, Magic.Length).SequenceEqual(Magic)
            || archivo[7] != VersionFormato)
            throw new LicenciaInvalidaException(noValida);

        var n = BinaryPrimitives.ReadUInt32BigEndian(archivo.AsSpan(OffsetLongitud, 4));
        // Firmado = encabezado + longitud + cifrado + tag. La firma (64 bytes) va al final.
        long finFirmado = OffsetCifrado + (long)n + TamanoTag;
        if (archivo.Length != finFirmado + TamanoFirma)
            throw new LicenciaInvalidaException(noValida);

        var firmado = archivo.AsSpan(0, (int)finFirmado);
        var firma = archivo.AsSpan((int)finFirmado, TamanoFirma);
        if (!clavePublica.VerifyData(firmado, firma, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation))
            throw new LicenciaInvalidaException("La firma de la licencia no es válida.");

        var claveAes = HKDF.DeriveKey(HashAlgorithmName.SHA256, appSecret, 32, InstalacionSalt(installationId), InfoClave);
        var texto = new byte[n];
        try
        {
            using var aes = new AesGcm(claveAes, TamanoTag);
            aes.Decrypt(
                archivo.AsSpan(OffsetNonce, TamanoNonce),
                archivo.AsSpan(OffsetCifrado, (int)n),
                archivo.AsSpan(OffsetCifrado + (int)n, TamanoTag),
                texto,
                archivo.AsSpan(0, TamanoEncabezado));
        }
        catch (CryptographicException)
        {
            throw new LicenciaInvalidaException("La licencia no corresponde a esta instalación.");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(claveAes);
        }

        LicenciaPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<LicenciaPayload>(texto, Opciones);
        }
        catch (JsonException)
        {
            throw new LicenciaInvalidaException(noValida);
        }

        if (payload is null || payload.InstallationId != installationId)
            throw new LicenciaInvalidaException("La licencia no corresponde a esta instalación.");

        ValidarReglas(payload);
        return payload;
    }

    // RFC 4122 en orden big-endian (hex de "N"). No usar Guid.ToByteArray(): mezcla endianness y no coincide con el emisor.
    private static byte[] InstalacionSalt(Guid id) => Convert.FromHexString(id.ToString("N"));

    private static void ValidarReglas(LicenciaPayload p)
    {
        if (p.Formato != VersionFormato || p.Serial < 1)
            throw new LicenciaInvalidaException("La licencia tiene un formato no compatible con esta versión.");
        if (string.IsNullOrWhiteSpace(p.Cliente) || p.Cliente.Length > 100
            || string.IsNullOrWhiteSpace(p.Edicion) || p.Edicion.Length > 20
            || p.MaxEmpresas < 1 || p.MaxUsuarios < 1 || p.GraceDays < 0)
            throw new LicenciaInvalidaException("Los datos de la licencia no son válidos.");

        switch (p.Plan)
        {
            case "lifetime":
                if (p.ValidTo is not null || p.GraceDays != 0 || p.MaxVersionMayor is null)
                    throw new LicenciaInvalidaException("La licencia lifetime tiene datos de vigencia incorrectos.");
                break;
            case "mensual":
            case "anual":
                if (p.ValidTo is null || p.ValidTo <= p.ValidFrom)
                    throw new LicenciaInvalidaException("La licencia tiene una vigencia no válida.");
                break;
            default:
                throw new LicenciaInvalidaException("El plan de la licencia no es válido.");
        }
    }
}

public class LicenciaInvalidaException(string mensaje) : Exception(mensaje);

// Campos del payload cifrado (PLAN-CLI-LICENCIAS.md §5). JSON en camelCase, sin distinguir mayúsculas.
public class LicenciaPayload
{
    public int Formato { get; set; }
    public Guid LicenseId { get; set; }
    public long Serial { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public Guid InstallationId { get; set; }
    public string Plan { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public int MaxEmpresas { get; set; }
    public int MaxUsuarios { get; set; }
    public DateTimeOffset Emitida { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public int GraceDays { get; set; }
    public int? MaxVersionMayor { get; set; }
}
