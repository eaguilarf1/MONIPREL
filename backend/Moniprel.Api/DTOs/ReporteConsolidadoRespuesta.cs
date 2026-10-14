namespace Moniprel.Api.DTOs;

public class ReporteConsolidadoRespuesta
{
    public int PeriodoEvaluacionId { get; set; }

    public string Periodo { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public int TotalEvaluaciones { get; set; }

    public int TotalBajo { get; set; }

    public int TotalModerado { get; set; }

    public int TotalAlto { get; set; }

    public decimal PuntajeGeneralPromedio { get; set; }

    public List<ReporteDimensionRespuesta> Dimensiones { get; set; }
        = new();
}