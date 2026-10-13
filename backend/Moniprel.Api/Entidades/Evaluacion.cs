namespace Moniprel.Api.Entidades;

public class Evaluacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int PeriodoEvaluacionId { get; set; }

    public PeriodoEvaluacion PeriodoEvaluacion { get; set; } = null!;

    public DateTime FechaInicio { get; set; } = DateTime.UtcNow;

    public DateTime? FechaFinalizacion { get; set; }

    public bool Finalizada { get; set; }

    public ICollection<Respuesta> Respuestas { get; set; }
        = new List<Respuesta>();

    public Resultado? Resultado { get; set; }
}