using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Services;

public class OrdenesService(IDbContextFactory<AppDbContext> dbFactory, SesionService sesion, CorreoOrdenService correo)
{
    // La BD y el sufijo vienen de la tabla EMPRESA, pero se validan antes de armar nombres de tabla.
    private static readonly Regex Identificador = new(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);

    public async Task<List<Empresa>> EmpresasDelRolAsync(int rolId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.RolesEmpresa
            .Where(re => re.RolId == rolId)
            .Join(db.Empresas, re => re.EmpresaId, e => e.Id, (re, e) => e)
            .Where(e => e.Estado == true)
            .OrderBy(e => e.Id)
            .ToListAsync();
    }

    public async Task<List<OrdenSae>> ListarAsync(Empresa empresa, string status)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var sql = $"""
            SELECT wo.IdOrder, wo.IdSupplier, p.NOMBRE AS Vendor, wo.OrdDate, wo.Status, wo.Total, obs.STR_OBS AS Observations
            FROM dbo.WIS_Orders wo
            INNER JOIN {Tabla(empresa, "PROV")} p ON p.CLAVE COLLATE DATABASE_DEFAULT = wo.IdSupplier
            INNER JOIN {Tabla(empresa, "COMPO")} o ON o.CVE_DOC COLLATE DATABASE_DEFAULT = wo.IdOrder
            LEFT JOIN {Tabla(empresa, "OBS_DOCC")} obs ON obs.CVE_OBS = o.CVE_OBS
            WHERE wo.EmpresaId = @empresaId AND wo.Status = @status
            ORDER BY wo.OrdDate
            """;
        return await db.Database.SqlQueryRaw<OrdenSae>(sql,
            new SqlParameter("@empresaId", empresa.Id),
            new SqlParameter("@status", status)).ToListAsync();
    }

    public async Task<(OrdenCabeceraSae? Cabecera, List<OrdenPartidaSae> Partidas)> DetalleAsync(Empresa empresa, string idOrder)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var cabecera = await db.Database.SqlQueryRaw<OrdenCabeceraSae>($"""
            SELECT c.CVE_DOC AS IdOrder, p.NOMBRE AS Supplier, c.CAN_TOT AS Total, c.FECHA_DOC AS DateOrder
            FROM {Tabla(empresa, "COMPO")} c
            INNER JOIN {Tabla(empresa, "PROV")} p ON c.CVE_CLPV = p.CLAVE
            WHERE c.CVE_DOC = @idOrder
            """, new SqlParameter("@idOrder", idOrder)).FirstOrDefaultAsync();

        var partidas = await db.Database.SqlQueryRaw<OrdenPartidaSae>($"""
            SELECT c.CVE_ART AS IdProd, i.DESCR AS Description, c.CANT AS Quantity, c.PREC AS Price,
                   c.TOTIMP4 AS VAT, c.DESCU AS Discount, c.TOT_PARTIDA AS TotalItem
            FROM {Tabla(empresa, "PAR_COMPO")} c
            INNER JOIN {Tabla(empresa, "INVE")} i ON c.CVE_ART = i.CVE_ART
            WHERE c.CVE_DOC = @idOrder
            ORDER BY c.NUM_PAR
            """, new SqlParameter("@idOrder", idOrder)).ToListAsync();

        return (cabecera, partidas);
    }

    public async Task<List<HistorialEntrada>> HistorialAsync(Empresa empresa, string idOrder)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var entradas = await db.Bitacora
            .Where(b => b.EmpresaId == empresa.Id && b.IdOrder == idOrder)
            .OrderByDescending(b => b.LogDate)
            .ToListAsync();

        var nombres = await db.Usuarios
            .ToDictionaryAsync(u => u.Id.ToString(), u => u.FullName ?? u.NickName ?? string.Empty);

        return entradas
            .Select(b => new HistorialEntrada(b.LogDate, b.Descr, nombres.GetValueOrDefault(b.UserId, b.UserId)))
            .ToList();
    }

    public async Task<ResultadoOperacion> ProcesarAsync(ClaimsPrincipal usuario, Empresa empresa, string idOrder, bool aprobar, string? comentario)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        // Validación de servidor: la UI también la aplica, pero no es suficiente.
        if (!usuario.IsInRole("Aprobador"))
            return new(false, "Solo el rol Aprobador puede procesar órdenes.");

        // El circuito puede seguir abierto después de desactivar al usuario, rol o empresa.
        if (!await sesion.EsActivaAsync(usuario))
            return new(false, "Tu usuario o rol está inactivo.");
        if (!await db.Empresas.AnyAsync(e => e.Id == empresa.Id && e.Estado == true))
            return new(false, "La empresa está inactiva.");

        var usuarioId = int.Parse(usuario.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rolId = int.Parse(usuario.FindFirstValue("RolId")!);
        if (!await db.RolesEmpresa.AnyAsync(re => re.RolId == rolId && re.EmpresaId == empresa.Id))
            return new(false, "La empresa no está asignada a tu rol.");

        comentario = comentario?.Trim();
        if (ValidarComentario(aprobar, comentario) is { } errorComentario)
            return new(false, errorComentario);

        var orden = await db.OrdenesCompra
            .FirstOrDefaultAsync(o => o.EmpresaId == empresa.Id && o.IdOrder == idOrder);
        if (orden is null || orden.Status != "P")
            return new(false, "La orden ya fue procesada o no existe.");

        await using var tx = await db.Database.BeginTransactionAsync();

        // El filtro Status = 'P' evita procesar dos veces la misma orden.
        var estado = aprobar ? "O" : "R";
        var filas = await db.OrdenesCompra
            .Where(o => o.EmpresaId == empresa.Id && o.IdOrder == idOrder && o.Status == "P")
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, estado));
        if (filas == 0)
            return new(false, "La orden ya fue procesada por otro usuario.");

        var cambioStatus = aprobar ? string.Empty : ", STATUS = 'C'";
        await db.Database.ExecuteSqlRawAsync(
            $"UPDATE {Tabla(empresa, "COMPO")} SET BLOQ = 'N'{cambioStatus} WHERE CVE_DOC = @idOrder",
            new SqlParameter("@idOrder", idOrder));

        var descripcion = aprobar ? "Aprobada" : "Rechazada";
        if (!string.IsNullOrEmpty(comentario))
            descripcion = $"{descripcion}: {comentario}";

        db.Bitacora.Add(new BitacoraEntrada
        {
            EmpresaId = empresa.Id,
            CorrelatorId = Guid.NewGuid().ToString(),
            LogDate = DateTime.Now,
            IdOrder = idOrder,
            Descr = descripcion,
            UserId = usuarioId.ToString(),
            IdSupplier = orden.IdSupplier
        });

        await db.SaveChangesAsync();
        await tx.CommitAsync();

        // Después del commit: si el correo falla, la orden ya quedó procesada.
        var nombre = usuario.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var proveedor = await ProveedorAsync(empresa, idOrder);
        var fallo = await correo.EnviarResultadoAsync(orden, nombre, await EmailUsuarioAsync(usuarioId), aprobar, comentario, proveedor);
        var mensaje = aprobar ? "Orden autorizada." : "Orden rechazada.";
        if (fallo is not null) mensaje += " (No se pudo enviar el correo.)";

        return new(true, mensaje);
    }

    private static string? ValidarComentario(bool aprobar, string? comentario)
    {
        if (!aprobar && string.IsNullOrEmpty(comentario)) return "Para rechazar la orden escribe un comentario.";
        if (comentario?.Length > 150) return "El comentario no puede exceder 150 caracteres.";
        return null;
    }

    // El Reply-To sale de la BD, no de un claim: así no hace falta volver a iniciar sesión.
    // Nombre del proveedor en SAE. Si SAE no responde, el correo usa la clave del proveedor.
    private async Task<string?> ProveedorAsync(Empresa empresa, string idOrder)
    {
        try
        {
            return (await DetalleAsync(empresa, idOrder)).Cabecera?.Supplier;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<string?> EmailUsuarioAsync(int usuarioId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Usuarios.Where(u => u.Id == usuarioId).Select(u => u.Email).FirstOrDefaultAsync();
    }

    private static string Tabla(Empresa empresa, string tabla)
    {
        if (!Identificador.IsMatch(empresa.DbSae ?? string.Empty) || !Identificador.IsMatch(empresa.Sufijo ?? string.Empty))
            throw new InvalidOperationException($"La empresa {empresa.Id} tiene una base o sufijo inválido.");

        return $"[{empresa.DbSae}].dbo.[{tabla}{empresa.Sufijo}]";
    }
}
