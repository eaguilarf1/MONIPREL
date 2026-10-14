namespace Moniprel.Api.DTOs;

public class ActualizarConfiguracionReporteSolicitud
{
    public string CorreoDestinatario { get; set; } = string.Empty;

    public bool EnvioAutomatico { get; set; }

    public int FrecuenciaDias { get; set; }
}