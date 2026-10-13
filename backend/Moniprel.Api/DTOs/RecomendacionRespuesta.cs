namespace Moniprel.Api.DTOs;

public class RecomendacionRespuesta
{
    public string Tipo { get; set; } = string.Empty;

    public string? Dimension { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;
}