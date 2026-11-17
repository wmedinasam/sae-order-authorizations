using SAEOrderAuthorizations.Web.Services;
using Xunit;

namespace SAEOrderAuthorizations.Tests;

public class LicenciaVigenciaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void MensualDentroDeVigenciaEsVigenteConDiasRestantes()
    {
        var datos = Mensual(validTo: Ahora.AddDays(10), graceDays: 7);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora, versionActual: 1);

        Assert.Equal(EstadoLicencia.Vigente, estado.Estado);
        Assert.Equal(10, estado.DiasRestantes);
        Assert.True(estado.PermiteEscritura);
    }

    [Fact]
    public void MensualVencidaDentroDeGraciaPermiteEscribirYCuentaElPlazo()
    {
        var datos = Mensual(validTo: Ahora.AddDays(-2), graceDays: 7);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora, versionActual: 1);

        Assert.Equal(EstadoLicencia.EnGracia, estado.Estado);
        Assert.Equal(5, estado.DiasRestantes);
        Assert.True(estado.PermiteEscritura);
    }

    [Fact]
    public void MensualPasadaLaGraciaEsSoloLectura()
    {
        var datos = Mensual(validTo: Ahora.AddDays(-10), graceDays: 7);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora, versionActual: 1);

        Assert.Equal(EstadoLicencia.Vencida, estado.Estado);
        Assert.False(estado.PermiteEscritura);
    }

    [Fact]
    public void LifetimeNuncaVenceNiAvisa()
    {
        var datos = Lifetime(maxVersionMayor: 1);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora.AddYears(20), versionActual: 1);

        Assert.Equal(EstadoLicencia.Vigente, estado.Estado);
        Assert.Null(estado.DiasRestantes);
        Assert.True(estado.PermiteEscritura);
    }

    [Fact]
    public void LifetimeNoCubreVersionMayorPosterior()
    {
        var datos = Lifetime(maxVersionMayor: 1);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora, versionActual: 2);

        Assert.Equal(EstadoLicencia.VersionNoCubierta, estado.Estado);
        Assert.False(estado.PermiteEscritura);
    }

    [Fact]
    public void LicenciaAunNoVigenteNoPermiteEscribir()
    {
        var datos = Mensual(validTo: Ahora.AddMonths(1), graceDays: 7);
        datos.ValidFrom = Ahora.AddDays(3);

        var estado = LicenciaService.EvaluarVigencia(datos, Ahora, versionActual: 1);

        Assert.Equal(EstadoLicencia.Invalida, estado.Estado);
        Assert.False(estado.PermiteEscritura);
    }

    private static LicenciaPayload Mensual(DateTimeOffset validTo, int graceDays) => new()
    {
        Formato = 1,
        LicenseId = Guid.NewGuid(),
        Serial = 1,
        Cliente = "Cliente de prueba",
        Plan = "mensual",
        Edicion = "pro",
        MaxEmpresas = 5,
        MaxUsuarios = 25,
        Emitida = Ahora.AddMonths(-1),
        ValidFrom = Ahora.AddMonths(-1),
        ValidTo = validTo,
        GraceDays = graceDays
    };

    private static LicenciaPayload Lifetime(int maxVersionMayor) => new()
    {
        Formato = 1,
        LicenseId = Guid.NewGuid(),
        Serial = 1,
        Cliente = "Cliente de prueba",
        Plan = "lifetime",
        Edicion = "pro",
        MaxEmpresas = 5,
        MaxUsuarios = 25,
        Emitida = Ahora.AddMonths(-1),
        ValidFrom = Ahora.AddMonths(-1),
        ValidTo = null,
        GraceDays = 0,
        MaxVersionMayor = maxVersionMayor
    };
}
