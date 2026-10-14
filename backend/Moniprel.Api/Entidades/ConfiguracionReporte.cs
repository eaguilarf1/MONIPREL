namespace Moniprel.Api.Entidades;

public class ConfiguracionReporte
{
    public int Id { get; set; }

    public string CorreoDestinatario { get; set; } = string.Empty;

    public bool EnvioAutomatico { get; set; } = false;

    public int FrecuenciaDias { get; set; } = 7;

    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public DateTime? UltimoEnvio { get; set; }
}