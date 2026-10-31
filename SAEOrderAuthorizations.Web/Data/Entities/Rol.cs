namespace SAEOrderAuthorizations.Web.Data.Entities;

public class Rol
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public bool? EsAdministrador { get; set; }
    public bool? Estado { get; set; }
}
