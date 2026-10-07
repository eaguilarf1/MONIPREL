namespace Moniprel.Api.DTOs;

public class ActualizarUsuarioSolicitud
{
    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public int RolId { get; set; }
}