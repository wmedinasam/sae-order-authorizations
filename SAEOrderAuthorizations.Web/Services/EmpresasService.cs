using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public class EmpresasService(AppDbContext db)
{
    public async Task<List<Empresa>> ListarAsync(bool incluirInactivas) =>
        await db.Empresas
            .Where(e => incluirInactivas || e.Estado == true)
            .OrderBy(e => e.Nombre)
            .ToListAsync();

    public async Task<ResultadoOperacion> GuardarAsync(ClaimsPrincipal usuario, EmpresaEdicion modelo)
    {
        if (!usuario.EsAdministrador())
            return new(false, "Solo un administrador puede modificar empresas.");

        modelo.Normalizar();
        var validacion = new List<ValidationResult>();
        if (!Validator.TryValidateObject(modelo, new ValidationContext(modelo), validacion, validateAllProperties: true))
            return new(false, validacion[0].ErrorMessage ?? "Datos de empresa no válidos.");

        // Sin índice único en BD (no hay cambios de esquema): la unicidad se valida aquí, entre activas e inactivas.
        var duplicada = await db.Empresas.AnyAsync(e =>
            e.Id != modelo.Id && e.DbSae == modelo.DbSae && e.Sufijo == modelo.Sufijo);
        if (duplicada)
            return new(false, "Ya existe una empresa con esa base y sufijo.");

        if (modelo.Id == 0)
        {
            db.Empresas.Add(new Empresa
            {
                Nombre = modelo.Nombre,
                Direccion = modelo.Direccion,
                DbSae = modelo.DbSae,
                Sufijo = modelo.Sufijo,
                Estado = true
            });
            await db.SaveChangesAsync();
            return new(true, "Empresa creada.");
        }

        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == modelo.Id);
        if (empresa is null)
            return new(false, "La empresa no existe.");

        empresa.Nombre = modelo.Nombre;
        empresa.Direccion = modelo.Direccion;
        empresa.DbSae = modelo.DbSae;
        empresa.Sufijo = modelo.Sufijo;
        await db.SaveChangesAsync();
        return new(true, "Empresa actualizada.");
    }

    // Baja lógica: no hay borrado físico. Estado = false saca la empresa de Órdenes.
    public async Task<ResultadoOperacion> CambiarEstadoAsync(ClaimsPrincipal usuario, int id, bool activa)
    {
        if (!usuario.EsAdministrador())
            return new(false, "Solo un administrador puede modificar empresas.");

        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == id);
        if (empresa is null)
            return new(false, "La empresa no existe.");

        empresa.Estado = activa;
        await db.SaveChangesAsync();
        return new(true, activa ? "Empresa reactivada." : "Empresa desactivada.");
    }
}

public class EmpresaEdicion
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(50, ErrorMessage = "La dirección no puede exceder 50 caracteres.")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "La base SAE es obligatoria.")]
    [StringLength(50, ErrorMessage = "La base SAE no puede exceder 50 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_]+$", ErrorMessage = "La base SAE solo admite letras, números y guion bajo.")]
    public string? DbSae { get; set; }

    [Required(ErrorMessage = "El sufijo es obligatorio.")]
    [StringLength(10, ErrorMessage = "El sufijo no puede exceder 10 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9_]+$", ErrorMessage = "El sufijo solo admite letras, números y guion bajo.")]
    public string? Sufijo { get; set; }

    // Quita espacios antes de validar: los valores de DbSae y Sufijo terminan armando nombres de tabla.
    public void Normalizar()
    {
        Nombre = Nombre?.Trim();
        Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim();
        DbSae = DbSae?.Trim();
        Sufijo = Sufijo?.Trim();
    }
}
