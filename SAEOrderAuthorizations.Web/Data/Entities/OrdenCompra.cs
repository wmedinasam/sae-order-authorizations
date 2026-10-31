namespace SAEOrderAuthorizations.Web.Data.Entities;

public class OrdenCompra
{
    public int Id { get; set; }
    public int? EmpresaId { get; set; }
    public string IdOrder { get; set; } = string.Empty;
    public string IdSupplier { get; set; } = string.Empty;
    public DateTime OrdDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public double Total { get; set; }
}
