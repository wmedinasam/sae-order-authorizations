using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

// Módulo Usuarios: solo create/update y soft delete (Estado). Un único rol por usuario.
public class UsuariosService(AppDbContext db)
{
    private static readonly ResultadoOperacion SinPermiso = new(false, "Solo un administrador puede gestionar usuarios.");

    public async Task<List<UsuarioFila>> ListarAsync(ClaimsPrincipal actor, bool incluirInactivos)
    {
        if (!actor.EsAdministrador()) return new();

        return await db.Usuarios
            .Where(u => incluirInactivos || u.Estado)
            .OrderBy(u => u.NickName)
            .Select(u => new UsuarioFila(u.Id, u.FullName, u.NickName, u.Email,
                db.Roles.Where(r => r.Id == u.RolId).Select(r => r.Nombre).FirstOrDefault(), u.Estado))
            .ToListAsync();
    }

    // Roles activos más el actual del usuario (si está inactivo) para que el select lo muestre.
    public async Task<List<Rol>> RolesAsignablesAsync(ClaimsPrincipal actor, int rolActual)
    {
        if (!actor.EsAdministrador()) return new();

        return await db.Roles
            .Where(r => r.Estado == true || r.Id == rolActual)
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    // Devuelve los datos sin la contraseña: el formulario de edición nunca la muestra.
    public async Task<UsuarioDatos?> ObtenerAsync(ClaimsPrincipal actor, int id)
    {
        if (!actor.EsAdministrador()) return null;

        return await db.Usuarios.AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDatos
            {
                FullName = u.FullName ?? string.Empty,
                NickName = u.NickName ?? string.Empty,
                Email = u.Email,
                SendMail = u.SendMail ?? false,
                RolId = u.RolId ?? 0,
                Estado = u.Estado
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ResultadoOperacion> GuardarAsync(ClaimsPrincipal actor, int? id, UsuarioDatos datos)
    {
        if (!actor.EsAdministrador()) return SinPermiso;

        var nombre = datos.FullName.Trim();
        var nick = datos.NickName.Trim();
        var email = string.IsNullOrWhiteSpace(datos.Email) ? null : datos.Email.Trim();
        // En edición, contraseña en blanco = conservar la actual. Nunca se registra en logs.
        var clave = string.IsNullOrWhiteSpace(datos.Password) ? null : datos.Password;

        if (nombre.Length is 0 or > 50) return new(false, "El nombre es obligatorio (máximo 50 caracteres).");
        if (nick.Length is 0 or > 15) return new(false, "El nick es obligatorio (máximo 15 caracteres).");
        if (id is null && clave is null) return new(false, "La contraseña es obligatoria al crear el usuario.");
        if (clave?.Length > 50) return new(false, "La contraseña no puede exceder 50 caracteres.");
        if (email is not null && (email.Length > 50 || !new EmailAddressAttribute().IsValid(email)))
            return new(false, "El correo no es válido (máximo 50 caracteres).");

        // Serializable: las comprobaciones de nick y de administradores no pueden colarse entre dos peticiones.
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (!await RolActivoAsync(datos.RolId))
            return new(false, "El rol seleccionado no existe o está inactivo.");

        var nickMinusculas = nick.ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Id != (id ?? 0) && u.NickName!.ToLower() == nickMinusculas))
            return new(false, "Ya existe un usuario con ese nick.");

        if (id is null)
        {
            db.Usuarios.Add(new Usuario
            {
                FullName = nombre,
                NickName = nick,
                Password = clave,
                Email = email,
                SendMail = datos.SendMail,
                RolId = datos.RolId,
                Estado = datos.Estado,
                ActivatedDate = DateTime.Now,
                UserActivates = actor.IdUsuario()
            });
        }
        else
        {
            var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return new(false, "El usuario no existe.");

            var error = await ErrorCambioAsync(actor, u, datos.Estado, datos.RolId);
            if (error is not null) return new(false, error);

            u.FullName = nombre;
            u.NickName = nick;
            u.Email = email;
            u.SendMail = datos.SendMail;
            u.RolId = datos.RolId;
            u.Estado = datos.Estado;
            if (clave is not null) u.Password = clave;
        }

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return new(true, id is null ? "Usuario creado." : "Usuario actualizado.");
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(ClaimsPrincipal actor, int id, bool activo)
    {
        if (!actor.EsAdministrador()) return SinPermiso;

        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        if (u is null) return new(false, "El usuario no existe.");

        if (activo && !await RolActivoAsync(u.RolId ?? 0))
            return new(false, "El rol del usuario está inactivo; asígnale un rol activo antes de reactivarlo.");

        var error = await ErrorCambioAsync(actor, u, activo, u.RolId ?? 0);
        if (error is not null) return new(false, error);

        u.Estado = activo;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return new(true, activo ? "Usuario reactivado." : "Usuario desactivado.");
    }

    // Salvaguardas de servidor: no desactivarse a uno mismo y no dejar el sistema sin administradores activos.
    private async Task<string?> ErrorCambioAsync(ClaimsPrincipal actor, Usuario u, bool estadoNuevo, int rolNuevo)
    {
        if (!estadoNuevo && u.Id == actor.IdUsuario())
            return "No puedes desactivarte a ti mismo.";

        var eraAdmin = await AdministradoresActivos().AnyAsync(a => a.Id == u.Id);
        var seraAdmin = estadoNuevo && await db.Roles.AnyAsync(r => r.Id == rolNuevo && r.EsAdministrador == true && r.Estado == true);
        if (eraAdmin && !seraAdmin && !await AdministradoresActivos().AnyAsync(a => a.Id != u.Id))
            return "No puedes desactivar ni quitar el rol de administrador al último administrador activo.";

        return null;
    }

    private Task<bool> RolActivoAsync(int rolId) =>
        db.Roles.AnyAsync(r => r.Id == rolId && r.Estado == true);

    // Usuarios activos cuyo rol es administrador y está activo (mismo criterio que el login).
    private IQueryable<Usuario> AdministradoresActivos() =>
        db.Usuarios.Where(u => u.Estado && db.Roles.Any(r => r.Id == u.RolId && r.EsAdministrador == true && r.Estado == true));
}

public record UsuarioFila(int Id, string? FullName, string? NickName, string? Email, string? Rol, bool Estado);

public class UsuarioDatos
{
    [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nick es obligatorio."), StringLength(15, ErrorMessage = "Máximo 15 caracteres.")]
    public string NickName { get; set; } = string.Empty;

    // Texto en claro (deuda temporal documentada en CLAUDE.md). Solo entra por formulario; nunca sale.
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Password { get; set; }

    [EmailAddress(ErrorMessage = "Correo no válido."), StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string? Email { get; set; }

    public bool SendMail { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un rol.")]
    public int RolId { get; set; }

    public bool Estado { get; set; } = true;
}
