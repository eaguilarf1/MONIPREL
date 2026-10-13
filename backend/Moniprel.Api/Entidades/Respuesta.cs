namespace Moniprel.Api.Entidades;

public class Respuesta
{
    public int Id { get; set; }

    public int EvaluacionId { get; set; }

    public Evaluacion Evaluacion { get; set; } = null!;

    public int PreguntaId { get; set; }

    public Pregunta Pregunta { get; set; } = null!;

    public int Valor { get; set; }
}