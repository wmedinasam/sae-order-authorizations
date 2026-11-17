namespace SAEOrderAuthorizations.Web.Data.Entities;

public class Marca
{
    public string Slot { get; set; } = string.Empty;
    public byte[]? Contenido { get; set; }
    public string? TipoMime { get; set; }
    public string? NombreArchivo { get; set; }
    public DateTime Actualizado { get; set; }
    public bool Estado { get; set; } = true;
}
