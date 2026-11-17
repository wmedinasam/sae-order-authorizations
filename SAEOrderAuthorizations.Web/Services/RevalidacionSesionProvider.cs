using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace SAEOrderAuthorizations.Web.Services;

// Circuitos de Blazor Server: un usuario desactivado deja de tener sesión en la pestaña abierta.
public class RevalidacionSesionProvider(ILoggerFactory loggerFactory, SesionService sesion)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    // shortcut: consulta a BD cada minuto por circuito, subir el intervalo si hay muchos usuarios simultáneos
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState estado, CancellationToken cancellationToken) =>
        sesion.EsActivaAsync(estado.User);
}
