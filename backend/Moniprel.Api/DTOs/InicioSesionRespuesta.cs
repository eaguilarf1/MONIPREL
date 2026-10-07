namespace Moniprel.Api.DTOs;

public class InicioSesionRespuesta
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEn { get; set; }

    public int UsuarioId { get; set; }

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;
}