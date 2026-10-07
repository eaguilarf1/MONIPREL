namespace Moniprel.Api.DTOs;

public class UsuarioRespuesta
{
    public int Id { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int RolId { get; set; }

    public string Rol { get; set; } = string.Empty;
}