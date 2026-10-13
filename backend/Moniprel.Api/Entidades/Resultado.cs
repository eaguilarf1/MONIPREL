namespace Moniprel.Api.Entidades;

public class Resultado
{
    public int Id { get; set; }

    public int EvaluacionId { get; set; }

    public Evaluacion Evaluacion { get; set; } = null!;

    public decimal PuntajeGeneral { get; set; }

    public string NivelRiesgo { get; set; } = string.Empty;

    public DateTime FechaCalculo { get; set; } = DateTime.UtcNow;

    public ICollection<ResultadoDimension> ResultadosDimension { get; set; }
        = new List<ResultadoDimension>();
}