using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Moniprel.Api.Configuraciones;
using Moniprel.Api.Interfaces;

namespace Moniprel.Api.Servicios;

public class ServicioCorreo : IServicioCorreo
{
    private readonly ConfiguracionSmtp _configuracion;

    public ServicioCorreo(
        IOptions<ConfiguracionSmtp> configuracion)
    {
        _configuracion = configuracion.Value;
    }

    public async Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpoHtml)
    {
        using var mensaje = new MailMessage();

        mensaje.From = new MailAddress(
            _configuracion.CorreoRemitente,
            _configuracion.NombreRemitente);

        mensaje.To.Add(destinatario);
        mensaje.Subject = asunto;
        mensaje.Body = cuerpoHtml;
        mensaje.IsBodyHtml = true;

        using var cliente = new SmtpClient(
            _configuracion.Host,
            _configuracion.Puerto);

        cliente.EnableSsl = _configuracion.UsarSsl;

        cliente.Credentials = new NetworkCredential(
            _configuracion.Usuario,
            _configuracion.Contrasena);

        await cliente.SendMailAsync(mensaje);
    }
}