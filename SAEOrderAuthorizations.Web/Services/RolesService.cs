using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public record RolFila(int Id, string? Nombre, string? Descripcion, bool EsAdministrador, bool Estado, int NumEmpresas);

public record RolFormulario(string? Nombre, string? Descripcion, bool EsAdministrador, bool Estado, List<int> EmpresaIds);

public class RolesService(AppDbContext db)
{
    // Las políticas de autorización (Admin, Aprobador) dependen de estos nombres.
    private static readonly string[] NombresReservados = ["Admin", "Aprobador"];

    public async Task<List<RolFila>> ListarAsync(ClaimsPrincipal usuario, bool incluirInactivos)
    {
        if (!usuario.EsAdministrador()) return new List<RolFila>();

        var consulta = db.Roles.AsQueryable();
        if (!incluirInactivos) consulta = consulta.Where(r => r.Estado == true);

        return await consulta
            .OrderBy(r => r.Id)
            .Select(r => new RolFila(r.Id, r.Nombre, r.Descripcion, r.EsAdministrador == true, r.Estado == true,
                db.RolesEmpresa.Count(re => re.RolId == r.Id)))
            .ToListAsync();
    }

    public async Task<List<Empresa>> EmpresasActivasAsync(ClaimsPrincipal usuario)
    {
        if (!usuario.EsAdministrador()) return new List<Empresa>();
        return await db.Empresas.Where(e => e.Estado == true).OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<List<int>> EmpresaIdsDelRolAsync(ClaimsPrincipal usuario, int rolId)
    {
        if (!usuario.EsAdministrador()) return new List<int>();
        return await db.RolesEmpresa.Where(re => re.RolId == rolId).Select(re => re.EmpresaId).ToListAsync();
    }

    public async Task<ResultadoOperacion> GuardarAsync(ClaimsPrincipal usuario, int? id, RolFormulario f)
    {
        if (!usuario.EsAdministrador()) return new(false, "Solo los administradores pueden gestionar roles.");

        var nombre = f.Nombre?.Trim() ?? string.Empty;
        var descripcion = f.Descripcion?.Trim() ?? string.Empty;
        if (nombre.Length == 0) return new(false, "El nombre es obligatorio.");
        if (nombre.Length > 50) return new(false, "El nombre no puede exceder 50 caracteres.");
        if (descripcion.Length > 50) return new(false, "La descripción no puede exceder 50 caracteres.");

        var actual = id is null ? null : await db.Roles.FindAsync(id.Value);
        if (id is not null && actual is null) return new(false, "El rol no existe.");

        var nombreActual = actual?.Nombre?.Trim() ?? string.Empty;
        if (NombresReservados.Contains(nombreActual) && nombre != nombreActual)
            return new(false, $"El rol {nombreActual} no puede renombrarse: las políticas de autorización dependen de su nombre.");

        var idActual = actual?.Id ?? 0;
        var otrosNombres = await db.Roles.Where(r => r.Id != idActual).Select(r => r.Nombre).ToListAsync();
        if (otrosNombres.Any(n => string.Equals(n?.Trim(), nombre, StringComparison.OrdinalIgnoreCase)))
            return new(false, "Ya existe un rol con ese nombre.");

        var error = await ValidarEstadoAsync(actual, f.EsAdministrador, f.Estado);
        if (error is not null) return new(false, error);

        // Solo se pueden asignar empresas activas; las inactivas no aparecen en el formulario.
        var activas = await db.Empresas.Where(e => e.Estado == true).Select(e => e.Id).ToListAsync();
        var seleccion = f.EmpresaIds.Distinct().ToList();
        if (!seleccion.All(activas.Contains)) return new(false, "Hay empresas no válidas o inactivas.");

        await using var tx = await db.Database.BeginTransactionAsync();

        var rol = actual ?? new Rol();
        if (actual is null) db.Roles.Add(rol);
        rol.Nombre = nombre;
        rol.Descripcion = descripcion.Length == 0 ? null : descripcion;
        rol.EsAdministrador = f.EsAdministrador;
        rol.Estado = f.Estado;
        await db.SaveChangesAsync();

        // Reemplazo del conjunto de ROL_EMPRESA del rol. Las filas de empresas inactivas no se tocan.
        await db.RolesEmpresa
            .Where(re => re.RolId == rol.Id && activas.Contains(re.EmpresaId) && !seleccion.Contains(re.EmpresaId))
            .ExecuteDeleteAsync();

        var existentes = await db.RolesEmpresa.Where(re => re.RolId == rol.Id).Select(re => re.EmpresaId).ToListAsync();
        db.RolesEmpresa.AddRange(seleccion.Except(existentes).Select(e => new RolEmpresa { RolId = rol.Id, EmpresaId = e }));
        await db.SaveChangesAsync();

        await tx.CommitAsync();
        return new(true, actual is null ? "Rol creado." : "Rol actualizado.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(ClaimsPrincipal usuario, int id, bool activo)
    {
        if (!usuario.EsAdministrador()) return new(false, "Solo los administradores pueden gestionar roles.");

        var rol = await db.Roles.FindAsync(id);
        if (rol is null) return new(false, "El rol no existe.");

        var error = await ValidarEstadoAsync(rol, rol.EsAdministrador == true, activo);
        if (error is not null) return new(false, error);

        rol.Estado = activo;
        await db.SaveChangesAsync();
        return new(true, activo ? "Rol reactivado." : "Rol desactivado.");
    }

    // Salvaguardas de estado: usuarios activos y último administrador activo.
    private async Task<string?> ValidarEstadoAsync(Rol? actual, bool esAdministrador, bool activo)
    {
        if (actual is null) return null;

        if (actual.Estado == true && !activo &&
            await db.Usuarios.AnyAsync(u => u.RolId == actual.Id && u.Estado))
            return "No se puede desactivar: el rol tiene usuarios activos.";

        var eraAdminActivo = actual.Estado == true && actual.EsAdministrador == true;
        var seguiraAdminActivo = activo && esAdministrador;
        if (eraAdminActivo && !seguiraAdminActivo &&
            !await db.Roles.AnyAsync(r => r.Id != actual.Id && r.Estado == true && r.EsAdministrador == true))
            return "Debe quedar al menos un rol administrador activo.";

        return null;
    }
}
