namespace Moniprel.Api.Entidades;

public class PeriodoEvaluacion
{
    public int Id { get; set; }

    public int InstrumentoId { get; set; }

    public Instrumento Instrumento { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public ICollection<Evaluacion> Evaluaciones { get; set; }
      = new List<Evaluacion>();
}