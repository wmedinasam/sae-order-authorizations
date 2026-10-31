namespace SAEOrderAuthorizations.Web.Data.Entities;

public class Configuracion
{
    public int Id { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public bool Estado { get; set; } = true;
}
