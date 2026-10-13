namespace Moniprel.Api.DTOs;

public class ResultadoEvaluacionRespuesta
{
    public int EvaluacionId { get; set; }

    public int PeriodoEvaluacionId { get; set; }

    public decimal PuntajeGeneral { get; set; }

    public string NivelRiesgo { get; set; } = string.Empty;

    public DateTime FechaCalculo { get; set; }

    public List<ResultadoDimensionRespuesta> Dimensiones { get; set; }
        = new();
}