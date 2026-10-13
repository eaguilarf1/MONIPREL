namespace Moniprel.Api.DTOs;

public class PreguntaDisponibleRespuesta
{
    public int Id { get; set; }

    public string Texto { get; set; } = string.Empty;

    public int Orden { get; set; }
}