namespace Moniprel.Api.DTOs;

public class CrearDimensionSolicitud
{
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public int Orden { get; set; }
}