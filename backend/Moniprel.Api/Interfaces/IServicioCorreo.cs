namespace Moniprel.Api.Interfaces;

public interface IServicioCorreo
{
    Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml);
}