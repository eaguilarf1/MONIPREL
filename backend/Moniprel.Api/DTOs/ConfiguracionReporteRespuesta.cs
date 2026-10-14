namespace Moniprel.Api.DTOs;

public class ConfiguracionReporteRespuesta
{
    public int Id { get; set; }

    public string CorreoDestinatario { get; set; } = string.Empty;

    public bool EnvioAutomatico { get; set; }

    public int FrecuenciaDias { get; set; }

    public DateTime FechaActualizacion { get; set; }

    public DateTime? UltimoEnvio { get; set; }
}