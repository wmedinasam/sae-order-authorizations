using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SAEOrderAuthorizations.Web.Services;
using Xunit;

namespace SAEOrderAuthorizations.Tests;

// El emisor de este archivo replica PLAN-CLI-LICENCIAS.md §4-5. Solo prueba coherencia interna:
// la compatibilidad real con wis-license se confirma con los vectores de prueba de ese repo.
public class LicenciaFormatoTests
{
    private static readonly Guid Instalacion = Guid.Parse("3f2c8a1e-9b7d-4c6a-8e5f-1a2b3c4d5e6f");
    private static readonly byte[] Secreto = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
    private static readonly JsonSerializerOptions Opciones = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Fact]
    public void AbreLicenciaMensualValida()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);

        var datos = Abrir(archivo, Instalacion, privada, Secreto);

        Assert.Equal("mensual", datos.Plan);
        Assert.Equal("Cliente de prueba", datos.Cliente);
        Assert.Equal(7, datos.GraceDays);
        Assert.NotNull(datos.ValidTo);
    }

    [Fact]
    public void AbreLicenciaLifetimeConVersionMayor()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Lifetime(), Instalacion, Secreto, privada);

        var datos = Abrir(archivo, Instalacion, privada, Secreto);

        Assert.Equal("lifetime", datos.Plan);
        Assert.Null(datos.ValidTo);
        Assert.Equal(1, datos.MaxVersionMayor);
    }

    [Fact]
    public void RechazaCualquierByteAlterado()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);
        var n = (int)BinaryPrimitives.ReadUInt32BigEndian(archivo.AsSpan(20, 4));
        // Encabezado, nonce, longitud, primer byte del cifrado, primer byte del tag, primer y último byte de la firma.
        int[] posiciones = [0, 8, 20, 24, 24 + n, 40 + n, archivo.Length - 1];

        foreach (var pos in posiciones)
        {
            var alterado = (byte[])archivo.Clone();
            alterado[pos] ^= 0x01;

            Assert.Throws<LicenciaInvalidaException>(() => Abrir(alterado, Instalacion, privada, Secreto));
        }
    }

    [Fact]
    public void RechazaOtraInstalacion()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Guid.NewGuid(), privada, Secreto));
    }

    [Fact]
    public void RechazaSecretoDeOtraApp()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);
        var otroSecreto = Secreto.Select(b => (byte)(b ^ 0xFF)).ToArray();

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Instalacion, privada, otroSecreto));
    }

    [Fact]
    public void RechazaFirmaDeOtraClave()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var otra = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);

        var ex = Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Instalacion, otra, Secreto));
        Assert.Contains("firma", ex.Message);
    }

    [Fact]
    public void RechazaArchivoTruncado()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo[..50], Instalacion, privada, Secreto));
    }

    [Fact]
    public void RechazaLifetimeConVencimiento()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = Lifetime();
        p.ValidTo = DateTimeOffset.UtcNow.AddYears(1);
        var archivo = Emitir(p, Instalacion, Secreto, privada);

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Instalacion, privada, Secreto));
    }

    [Fact]
    public void RechazaMensualSinVencimiento()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = Mensual();
        p.ValidTo = null;
        var archivo = Emitir(p, Instalacion, Secreto, privada);

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Instalacion, privada, Secreto));
    }

    [Fact]
    public void RechazaFormatoNoSoportado()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = Mensual();
        p.Formato = 2;
        var archivo = Emitir(p, Instalacion, Secreto, privada);

        Assert.Throws<LicenciaInvalidaException>(() => Abrir(archivo, Instalacion, privada, Secreto));
    }

    [Fact]
    public void AceptaClavePublicaEnFormatoPem()
    {
        using var privada = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var archivo = Emitir(Mensual(), Instalacion, Secreto, privada);
        using var publica = ECDsa.Create();
        publica.ImportFromPem(privada.ExportSubjectPublicKeyInfoPem());

        var datos = Abrir(archivo, Instalacion, publica, Secreto);

        Assert.Equal(1, datos.Formato);
    }

    private static LicenciaPayload Mensual() => new()
    {
        Formato = 1,
        LicenseId = Guid.NewGuid(),
        Serial = 1,
        Cliente = "Cliente de prueba",
        InstallationId = Instalacion,
        Plan = "mensual",
        Edicion = "pro",
        MaxEmpresas = 5,
        MaxUsuarios = 25,
        Emitida = DateTimeOffset.UtcNow,
        ValidFrom = DateTimeOffset.UtcNow.AddMinutes(-1),
        ValidTo = DateTimeOffset.UtcNow.AddMonths(1),
        GraceDays = 7,
        MaxVersionMayor = null
    };

    private static LicenciaPayload Lifetime() => new()
    {
        Formato = 1,
        LicenseId = Guid.NewGuid(),
        Serial = 1,
        Cliente = "Cliente de prueba",
        InstallationId = Instalacion,
        Plan = "lifetime",
        Edicion = "pro",
        MaxEmpresas = 5,
        MaxUsuarios = 25,
        Emitida = DateTimeOffset.UtcNow,
        ValidFrom = DateTimeOffset.UtcNow.AddMinutes(-1),
        ValidTo = null,
        GraceDays = 0,
        MaxVersionMayor = 1
    };

    private static LicenciaPayload Abrir(byte[] archivo, Guid instalacion, ECDsa clave, byte[] secreto) =>
        LicenciaFormato.Abrir(archivo, instalacion, clave, secreto);

    private static byte[] Emitir(LicenciaPayload payload, Guid instalacion, byte[] secreto, ECDsa privada)
    {
        var texto = JsonSerializer.SerializeToUtf8Bytes(payload, Opciones);

        var encabezado = new byte[20];
        Encoding.ASCII.GetBytes("WISLIC1").CopyTo(encabezado, 0);
        encabezado[7] = 1;
        RandomNumberGenerator.Fill(encabezado.AsSpan(8, 12));

        var clave = HKDF.DeriveKey(HashAlgorithmName.SHA256, secreto, 32,
            Convert.FromHexString(instalacion.ToString("N")), Encoding.ASCII.GetBytes("WISLIC1-aes"));
        var cifrado = new byte[texto.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(clave, 16))
            aes.Encrypt(encabezado.AsSpan(8, 12), texto, cifrado, tag, encabezado);

        var longitud = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(longitud, (uint)cifrado.Length);

        var firmado = encabezado.Concat(longitud).Concat(cifrado).Concat(tag).ToArray();
        var firma = privada.SignData(firmado, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return firmado.Concat(firma).ToArray();
    }
}
