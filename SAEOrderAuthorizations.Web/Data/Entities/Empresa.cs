namespace SAEOrderAuthorizations.Web.Data.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? DbSae { get; set; }
    public string? Sufijo { get; set; }
    public bool? Estado { get; set; }
}
