namespace Moniprel.Api.DTOs;

public class ReporteDimensionRespuesta
{
    public int DimensionId { get; set; }

    public string Dimension { get; set; } = string.Empty;

    public decimal PuntajePromedio { get; set; }
}