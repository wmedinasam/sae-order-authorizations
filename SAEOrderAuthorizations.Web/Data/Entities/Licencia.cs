namespace SAEOrderAuthorizations.Web.Data.Entities;

// Historial de licencias cargadas. La vigente es la última fila; el archivo se guarda para volver a verificarlo.
public class Licencia
{
    public int Id { get; set; }
    public Guid LicenseId { get; set; }
    public long Serial { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public string Edicion { get; set; } = string.Empty;
    public int MaxEmpresas { get; set; }
    public int MaxUsuarios { get; set; }
    public DateTime Emitida { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int GraceDays { get; set; }
    public int? MaxVersionMayor { get; set; }
    public byte[] Archivo { get; set; } = [];
    public DateTime CargadaUtc { get; set; }
    public int CargadaPor { get; set; }
}

// Una sola fila por BD: identifica la instalación y guarda el último acceso para detectar retrocesos de reloj.
public class InstalacionLicencia
{
    public int Id { get; set; }
    public Guid InstallationId { get; set; }
    public DateTime? UltimoVisto { get; set; }
}
