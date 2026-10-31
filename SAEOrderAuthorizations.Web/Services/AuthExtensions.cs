using System.Security.Claims;

namespace SAEOrderAuthorizations.Web.Services;

public static class AuthExtensions
{
    public static bool EsAdministrador(this ClaimsPrincipal usuario) =>
        usuario.HasClaim("EsAdministrador", "True");

    public static int IdUsuario(this ClaimsPrincipal usuario) =>
        int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
