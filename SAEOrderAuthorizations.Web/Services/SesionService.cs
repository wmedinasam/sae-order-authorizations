using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;

namespace SAEOrderAuthorizations.Web.Services;

// La cookie no sabe si el usuario o su rol se desactivaron después del login; esto lo consulta en BD.
public class SesionService(IDbContextFactory<AppDbContext> dbFactory)
{
    public async Task<bool> EsActivaAsync(ClaimsPrincipal usuario)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        if (!int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId))
            return false;

        return await (from u in db.Usuarios
                      join r in db.Roles on u.RolId equals r.Id
                      where u.Id == usuarioId && u.Estado && r.Estado == true
                      select u.Id).AnyAsync();
    }
}
