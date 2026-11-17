using System.Security.Claims;

namespace SAEOrderAuthorizations.Web.Services;

public record MarcaInfo(string LogoSidebarUrl, string LogoLoginUrl, string FaviconUrl, string FaviconTipo, string NombreEmpresa);

// Contrato de Fase 0: firmas estables para que los agentes compilen contra ellas. Cuerpos en M1.
public class MarcaService
{
    public static readonly string[] Slots = ["LogoSidebar", "LogoLogin", "Favicon"];

    public Task<MarcaInfo> ObtenerAsync() => throw new NotImplementedException();

    public Task<ResultadoOperacion> GuardarAsync(ClaimsPrincipal usuario, string slot, Stream contenido, string nombreArchivo) =>
        throw new NotImplementedException();

    public Task<ResultadoOperacion> RestaurarAsync(ClaimsPrincipal usuario, string slot) => throw new NotImplementedException();

    public Task<ResultadoOperacion> GuardarNombreAsync(ClaimsPrincipal usuario, string? nombre) => throw new NotImplementedException();
}
