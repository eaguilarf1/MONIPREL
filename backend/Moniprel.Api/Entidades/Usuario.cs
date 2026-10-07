namespace Moniprel.Api.Entidades;

public class Usuario
{
    public int Id { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string ContrasenaHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public ICollection<AsignacionOrganizacional> AsignacionesComoSupervisor { get; set; }
        = new List<AsignacionOrganizacional>();

    public ICollection<AsignacionOrganizacional> AsignacionesComoColaborador { get; set; }
        = new List<AsignacionOrganizacional>();
}