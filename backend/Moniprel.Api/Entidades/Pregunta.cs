namespace Moniprel.Api.Entidades;

public class Pregunta
{
    public int Id { get; set; }

    public int DimensionId { get; set; }

    public Dimension Dimension { get; set; } = null!;

    public string Texto { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool EsProtectora { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<Respuesta> Respuestas { get; set; }
      = new List<Respuesta>();
}