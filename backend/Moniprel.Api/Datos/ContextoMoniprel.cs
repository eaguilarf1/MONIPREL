using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Datos;

public class ContextoMoniprel : DbContext
{
    public ContextoMoniprel(DbContextOptions<ContextoMoniprel> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<AsignacionOrganizacional> AsignacionesOrganizacionales =>
        Set<AsignacionOrganizacional>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entidad =>
        {
            entidad.ToTable("roles");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entidad.HasIndex(x => x.Nombre)
                .IsUnique();

            entidad.Property(x => x.Descripcion)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Usuario>(entidad =>
        {
            entidad.ToTable("usuarios");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Nombres)
                .HasMaxLength(100)
                .IsRequired();

            entidad.Property(x => x.Apellidos)
                .HasMaxLength(100)
                .IsRequired();

            entidad.Property(x => x.Correo)
                .HasMaxLength(150)
                .IsRequired();

            entidad.HasIndex(x => x.Correo)
                .IsUnique();

            entidad.Property(x => x.ContrasenaHash)
                .HasMaxLength(500)
                .IsRequired();

            entidad.HasOne(x => x.Rol)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.RolId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AsignacionOrganizacional>(entidad =>
        {
            entidad.ToTable("asignaciones_organizacionales");

            entidad.HasKey(x => x.Id);

            entidad.HasIndex(x => new
            {
                x.SupervisorId,
                x.ColaboradorId
            })
            .IsUnique();

            entidad.HasOne(x => x.Supervisor)
                .WithMany(x => x.AsignacionesComoSupervisor)
                .HasForeignKey(x => x.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasOne(x => x.Colaborador)
                .WithMany(x => x.AsignacionesComoColaborador)
                .HasForeignKey(x => x.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Rol>().HasData(
            new Rol
            {
                Id = 1,
                Nombre = "Administrador",
                Descripcion = "Administración y configuración del sistema",
                Activo = true
            },
            new Rol
            {
                Id = 2,
                Nombre = "Colaborador",
                Descripcion = "Participa en las evaluaciones de monitoreo preventivo",
                Activo = true
            },
            new Rol
            {
                Id = 3,
                Nombre = "Supervisor",
                Descripcion = "Referencia organizacional de colaboradores asignados",
                Activo = true
            }
        );
    }
}