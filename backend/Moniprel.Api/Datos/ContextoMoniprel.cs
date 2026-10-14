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

    public DbSet<Instrumento> Instrumentos => Set<Instrumento>();

    public DbSet<Dimension> Dimensiones => Set<Dimension>();

    public DbSet<Pregunta> Preguntas => Set<Pregunta>();

    public DbSet<PeriodoEvaluacion> PeriodosEvaluacion =>
        Set<PeriodoEvaluacion>();

    public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();

    public DbSet<Respuesta> Respuestas => Set<Respuesta>();

    public DbSet<Resultado> Resultados => Set<Resultado>();

    public DbSet<ResultadoDimension> ResultadosDimension =>
        Set<ResultadoDimension>();

    public DbSet<ConfiguracionReporte> ConfiguracionesReporte =>
        Set<ConfiguracionReporte>();

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

        modelBuilder.Entity<Instrumento>(entidad =>
        {
            entidad.ToTable("instrumentos");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entidad.Property(x => x.Descripcion)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Dimension>(entidad =>
        {
            entidad.ToTable("dimensiones");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entidad.Property(x => x.Descripcion)
                .HasMaxLength(500);

            entidad.HasOne(x => x.Instrumento)
                .WithMany(x => x.Dimensiones)
                .HasForeignKey(x => x.InstrumentoId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasIndex(x => new
            {
                x.InstrumentoId,
                x.Orden
            })
            .IsUnique();
        });

        modelBuilder.Entity<Pregunta>(entidad =>
        {
            entidad.ToTable("preguntas");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Texto)
                .HasMaxLength(500)
                .IsRequired();

            entidad.HasOne(x => x.Dimension)
                .WithMany(x => x.Preguntas)
                .HasForeignKey(x => x.DimensionId)
                .OnDelete(DeleteBehavior.Restrict);

            entidad.HasIndex(x => new
            {
                x.DimensionId,
                x.Orden
            })
            .IsUnique();
        });

        modelBuilder.Entity<PeriodoEvaluacion>(entidad =>
        {
            entidad.ToTable("periodos_evaluacion");

            entidad.HasKey(x => x.Id);

            entidad.Property(x => x.Nombre)
                .HasMaxLength(150)
                .IsRequired();

            entidad.HasOne(x => x.Instrumento)
                .WithMany(x => x.Periodos)
                .HasForeignKey(x => x.InstrumentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

modelBuilder.Entity<Evaluacion>(entidad =>
{
    entidad.ToTable("evaluaciones");

    entidad.HasKey(x => x.Id);

    entidad.HasOne(x => x.Usuario)
        .WithMany(x => x.Evaluaciones)
        .HasForeignKey(x => x.UsuarioId)
        .OnDelete(DeleteBehavior.Restrict);

    entidad.HasOne(x => x.PeriodoEvaluacion)
        .WithMany(x => x.Evaluaciones)
        .HasForeignKey(x => x.PeriodoEvaluacionId)
        .OnDelete(DeleteBehavior.Restrict);

    entidad.HasIndex(x => new
    {
        x.UsuarioId,
        x.PeriodoEvaluacionId
    })
    .IsUnique();
});

modelBuilder.Entity<Respuesta>(entidad =>
{
    entidad.ToTable("respuestas");

    entidad.HasKey(x => x.Id);

    entidad.Property(x => x.Valor)
        .IsRequired();

    entidad.HasOne(x => x.Evaluacion)
        .WithMany(x => x.Respuestas)
        .HasForeignKey(x => x.EvaluacionId)
        .OnDelete(DeleteBehavior.Cascade);

    entidad.HasOne(x => x.Pregunta)
        .WithMany(x => x.Respuestas)
        .HasForeignKey(x => x.PreguntaId)
        .OnDelete(DeleteBehavior.Restrict);

    entidad.HasIndex(x => new
    {
        x.EvaluacionId,
        x.PreguntaId
    })
    .IsUnique();
});

modelBuilder.Entity<Resultado>(entidad =>
{
    entidad.ToTable("resultados");

    entidad.HasKey(x => x.Id);

    entidad.Property(x => x.PuntajeGeneral)
        .HasPrecision(4, 2);

    entidad.Property(x => x.NivelRiesgo)
        .HasMaxLength(20)
        .IsRequired();

    entidad.HasOne(x => x.Evaluacion)
        .WithOne(x => x.Resultado)
        .HasForeignKey<Resultado>(x => x.EvaluacionId)
        .OnDelete(DeleteBehavior.Cascade);

    entidad.HasIndex(x => x.EvaluacionId)
        .IsUnique();
});

modelBuilder.Entity<ResultadoDimension>(entidad =>
{
    entidad.ToTable("resultados_dimension");

    entidad.HasKey(x => x.Id);

    entidad.Property(x => x.Puntaje)
        .HasPrecision(4, 2);

    entidad.HasOne(x => x.Resultado)
        .WithMany(x => x.ResultadosDimension)
        .HasForeignKey(x => x.ResultadoId)
        .OnDelete(DeleteBehavior.Cascade);

    entidad.HasOne(x => x.Dimension)
        .WithMany(x => x.ResultadosDimension)
        .HasForeignKey(x => x.DimensionId)
        .OnDelete(DeleteBehavior.Restrict);

    entidad.HasIndex(x => new
    {
        x.ResultadoId,
        x.DimensionId
    })
    .IsUnique();
});

modelBuilder.Entity<ConfiguracionReporte>(entidad =>
{
    entidad.ToTable("configuraciones_reporte");

    entidad.HasKey(x => x.Id);

    entidad.Property(x => x.CorreoDestinatario)
        .HasMaxLength(150)
        .IsRequired();

    entidad.Property(x => x.FrecuenciaDias)
        .IsRequired();
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