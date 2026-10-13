namespace Moniprel.Api.DTOs;

public class EvaluacionDisponibleRespuesta
{
    public int PeriodoEvaluacionId { get; set; }

    public string Periodo { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int InstrumentoId { get; set; }

    public string Instrumento { get; set; } = string.Empty;

    public string? DescripcionInstrumento { get; set; }

    public List<DimensionDisponibleRespuesta> Dimensiones { get; set; }
        = new();
}