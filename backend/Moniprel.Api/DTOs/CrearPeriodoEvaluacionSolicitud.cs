namespace Moniprel.Api.DTOs;

public class CrearPeriodoEvaluacionSolicitud
{
    public int InstrumentoId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }
}