namespace SAEOrderAuthorizations.Web.Data.Entities;

public class BitacoraEntrada
{
    public int Id { get; set; }
    public int? EmpresaId { get; set; }
    public string CorrelatorId { get; set; } = string.Empty;
    public DateTime LogDate { get; set; }
    public string IdOrder { get; set; } = string.Empty;
    public string Descr { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? IdSupplier { get; set; }
}
