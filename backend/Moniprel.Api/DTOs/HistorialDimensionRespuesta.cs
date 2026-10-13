namespace Moniprel.Api.DTOs;

public class HistorialDimensionRespuesta
{
    public int DimensionId { get; set; }

    public string Dimension { get; set; } = string.Empty;

    public decimal Puntaje { get; set; }
}