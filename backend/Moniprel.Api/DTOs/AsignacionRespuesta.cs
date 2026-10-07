namespace Moniprel.Api.DTOs;

public class AsignacionRespuesta
{
    public int Id { get; set; }

    public int SupervisorId { get; set; }

    public string Supervisor { get; set; } = string.Empty;

    public int ColaboradorId { get; set; }

    public string Colaborador { get; set; } = string.Empty;

    public bool Activa { get; set; }

    public DateTime FechaCreacion { get; set; }
}