namespace Moniprel.Api.DTOs;

public class RealizarEvaluacionSolicitud
{
    public int PeriodoEvaluacionId { get; set; }

    public List<RespuestaEvaluacionSolicitud> Respuestas { get; set; }
        = new();
}