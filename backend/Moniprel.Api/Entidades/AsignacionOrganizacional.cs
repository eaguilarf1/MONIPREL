namespace Moniprel.Api.Entidades;

public class AsignacionOrganizacional
{
    public int Id { get; set; }

    public int SupervisorId { get; set; }

    public Usuario Supervisor { get; set; } = null!;

    public int ColaboradorId { get; set; }

    public Usuario Colaborador { get; set; } = null!;

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}