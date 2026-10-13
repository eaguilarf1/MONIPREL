namespace Moniprel.Api.Entidades;

public class Dimension
{
    public int Id { get; set; }

    public int InstrumentoId { get; set; }

    public Instrumento Instrumento { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Orden { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<Pregunta> Preguntas { get; set; }
        = new List<Pregunta>();

    public ICollection<ResultadoDimension> ResultadosDimension { get; set; }
        = new List<ResultadoDimension>();
}