using Microsoft.EntityFrameworkCore;
using SAEOrderAuthorizations.Web.Data.Entities;

namespace SAEOrderAuthorizations.Web.Data;

// Mapeo a las tablas legacy de ORDENES_COMPRA (ver ORD_SAE/SCRIPTS/Script Master MultiEmpresas.sql).
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<RolEmpresa> RolesEmpresa => Set<RolEmpresa>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<BitacoraEntrada> Bitacora => Set<BitacoraEntrada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>(e =>
        {
            e.ToTable("EMPRESA");
            e.Property(x => x.Nombre).HasMaxLength(50);
            e.Property(x => x.Direccion).HasMaxLength(50);
            e.Property(x => x.DbSae).HasColumnName("DB_SAE").HasMaxLength(50);
            e.Property(x => x.Sufijo).HasMaxLength(10);
            e.HasData(new Empresa { Id = 1, Nombre = "Empresa 01", DbSae = "SAE80Empre01", Sufijo = "01", Estado = true });
        });

        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("ROL");
            e.Property(x => x.Nombre).HasMaxLength(50);
            e.Property(x => x.Descripcion).HasMaxLength(50);
            e.Property(x => x.EsAdministrador).HasColumnName("EsAdministrador");
            e.HasData(
                new Rol { Id = 1, Nombre = "Admin", Descripcion = "Administrador", EsAdministrador = true, Estado = true },
                new Rol { Id = 2, Nombre = "Operador", Descripcion = "Operador", EsAdministrador = false, Estado = true },
                new Rol { Id = 3, Nombre = "Aprobador", Descripcion = "Aprobador de órdenes", EsAdministrador = false, Estado = true });
        });

        // Legacy no define clave primaria en ROL_EMPRESA; se usa la pareja como clave compuesta.
        modelBuilder.Entity<RolEmpresa>(e =>
        {
            e.ToTable("ROL_EMPRESA");
            e.HasKey(x => new { x.RolId, x.EmpresaId });
            e.HasData(
                new RolEmpresa { RolId = 1, EmpresaId = 1 },
                new RolEmpresa { RolId = 2, EmpresaId = 1 },
                new RolEmpresa { RolId = 3, EmpresaId = 1 });
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("WIS_USER");
            e.Property(x => x.FullName).HasColumnName("FULL_NAME").HasMaxLength(50);
            e.Property(x => x.NickName).HasColumnName("NICK_NAME").HasMaxLength(15);
            e.Property(x => x.Password).HasColumnName("PASSWORD").HasMaxLength(50);
            e.Property(x => x.Email).HasColumnName("EMAIL").HasMaxLength(50);
            e.Property(x => x.SendMail).HasColumnName("SEND_MAIL");
            e.Property(x => x.RolId).HasColumnName("ROL_ID");
            e.Property(x => x.ActivatedDate).HasColumnName("ACTIVATED_DATE");
            e.Property(x => x.UserActivates).HasColumnName("USER_ACTIVATES");
            // Contraseña en claro (temporal). El script maestro traía el cifrado AES legacy.
            e.HasData(
                new Usuario
                {
                    Id = 1, FullName = "administrador", NickName = "admin", Password = "123",
                    Email = "admin@admin.com", SendMail = false, RolId = 1, ActivatedDate = new DateTime(2026, 10, 31), UserActivates = 1
                },
                new Usuario
                {
                    Id = 2, FullName = "operador", NickName = "operador", Password = "123",
                    Email = "operador@admin.com", SendMail = false, RolId = 2, ActivatedDate = new DateTime(2026, 10, 31), UserActivates = 1
                },
                new Usuario
                {
                    Id = 3, FullName = "aprobador", NickName = "aprobador", Password = "123",
                    Email = "aprobador@admin.com", SendMail = false, RolId = 3, ActivatedDate = new DateTime(2026, 10, 31), UserActivates = 1
                });
        });

        modelBuilder.Entity<Configuracion>(e =>
        {
            e.ToTable("WIS_CONFIGURATION");
            e.Property(x => x.Key).HasColumnName("KEY").HasMaxLength(25);
            e.Property(x => x.Value).HasColumnName("VALUE").HasMaxLength(255);
            e.HasData(
                new Configuracion { Id = 1, Key = "MailFrom", Value = "" },
                new Configuracion { Id = 2, Key = "MailPassword", Value = "" },
                new Configuracion { Id = 3, Key = "MailSubject", Value = "" },
                new Configuracion { Id = 4, Key = "MailBody", Value = "" });
        });

        modelBuilder.Entity<OrdenCompra>(e =>
        {
            e.ToTable("WIS_Orders");
            e.Property(x => x.IdOrder).HasMaxLength(20).IsRequired();
            e.Property(x => x.IdSupplier).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasMaxLength(1).IsRequired();
            e.Property(x => x.Total).IsRequired();
            e.Property(x => x.OrdDate).IsRequired();
        });

        modelBuilder.Entity<BitacoraEntrada>(e =>
        {
            e.ToTable("WIS_WorkLog");
            e.Property(x => x.CorrelatorId).HasMaxLength(250).IsRequired();
            e.Property(x => x.IdOrder).HasMaxLength(25).IsRequired();
            e.Property(x => x.Descr).HasMaxLength(250).IsRequired();
            e.Property(x => x.UserId).HasMaxLength(50).IsRequired();
            e.Property(x => x.IdSupplier).HasColumnType("nchar(10)");
            e.Property(x => x.LogDate).IsRequired();
        });
    }
}
