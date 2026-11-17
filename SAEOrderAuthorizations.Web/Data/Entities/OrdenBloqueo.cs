namespace SAEOrderAuthorizations.Web.Data.Entities;

// Reserva exclusiva: un aprobador a la vez revisa una orden. Expira sola si el circuito muere.
public class OrdenBloqueo
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string IdOrder { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime Expira { get; set; }
    public DateTime Actualizado { get; set; }
}
