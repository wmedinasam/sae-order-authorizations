namespace SAEOrderAuthorizations.Web.Data.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? NickName { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public bool? SendMail { get; set; }
    public int? RolId { get; set; }
    public DateTime? ActivatedDate { get; set; }
    public int? UserActivates { get; set; }
    public bool Estado { get; set; } = true;
}
