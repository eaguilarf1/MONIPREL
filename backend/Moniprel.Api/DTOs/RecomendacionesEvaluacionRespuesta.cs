namespace Moniprel.Api.DTOs;

public class RecomendacionesEvaluacionRespuesta
{
    public int EvaluacionId { get; set; }

    public decimal PuntajeGeneral { get; set; }

    public string NivelRiesgo { get; set; } = string.Empty;

    public List<RecomendacionRespuesta> Recomendaciones { get; set; }
        = new();
}