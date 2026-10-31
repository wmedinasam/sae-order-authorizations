namespace SAEOrderAuthorizations.Web.Services;

// Filas de consultas a la BD de Aspel SAE (sin tabla propia; se leen con SqlQueryRaw).
public class OrdenSae
{
    public string IdOrder { get; set; } = string.Empty;
    public string IdSupplier { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public DateTime OrdDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public double Total { get; set; }
    public string? Observations { get; set; }
}

public class OrdenCabeceraSae
{
    public string IdOrder { get; set; } = string.Empty;
    public string? Supplier { get; set; }
    public double? Total { get; set; }
    public DateTime DateOrder { get; set; }
}

public class OrdenPartidaSae
{
    public string? IdProd { get; set; }
    public string? Description { get; set; }
    public double? Quantity { get; set; }
    public double? Price { get; set; }
    public double? VAT { get; set; }
    public double? Discount { get; set; }
    public double? TotalItem { get; set; }
}

public record HistorialEntrada(DateTime Fecha, string Descripcion, string Usuario);

public record ResultadoOperacion(bool Exito, string Mensaje);
