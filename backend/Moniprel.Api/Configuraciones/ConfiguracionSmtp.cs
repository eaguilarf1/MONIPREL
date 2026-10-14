namespace Moniprel.Api.Configuraciones;

public class ConfiguracionSmtp
{
    public string Host { get; set; } = string.Empty;

    public int Puerto { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public bool UsarSsl { get; set; }

    public string CorreoRemitente { get; set; } = string.Empty;

    public string NombreRemitente { get; set; } = string.Empty;
}