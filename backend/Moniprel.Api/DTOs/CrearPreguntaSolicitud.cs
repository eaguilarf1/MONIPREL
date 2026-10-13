namespace Moniprel.Api.DTOs;

public class CrearPreguntaSolicitud
{
    public string Texto { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool EsProtectora { get; set; }
}