using Moniprel.Api.Entidades;

namespace Moniprel.Api.Interfaces;

public interface IServicioToken
{
    string GenerarToken(Usuario usuario, DateTime fechaExpiracion);
}