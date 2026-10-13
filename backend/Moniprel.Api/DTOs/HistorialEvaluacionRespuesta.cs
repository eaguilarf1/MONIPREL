namespace Moniprel.Api.DTOs;

public class HistorialEvaluacionRespuesta
{
    public int EvaluacionId { get; set; }

    public int PeriodoEvaluacionId { get; set; }

    public string Periodo { get; set; } = string.Empty;

    public DateTime FechaInicioPeriodo { get; set; }

    public DateTime FechaFinPeriodo { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public decimal PuntajeGeneral { get; set; }

    public string NivelRiesgo { get; set; } = string.Empty;

    public List<HistorialDimensionRespuesta> Dimensiones { get; set; }
        = new();
}