namespace Moniprel.Api.DTOs;

public class DimensionDisponibleRespuesta
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Orden { get; set; }

    public List<PreguntaDisponibleRespuesta> Preguntas { get; set; }
        = new();
}