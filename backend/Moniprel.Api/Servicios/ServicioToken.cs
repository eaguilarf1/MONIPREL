using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Moniprel.Api.Entidades;
using Moniprel.Api.Interfaces;

namespace Moniprel.Api.Servicios;

public class ServicioToken : IServicioToken
{
    private readonly IConfiguration _configuracion;

    public ServicioToken(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public string GenerarToken(Usuario usuario, DateTime fechaExpiracion)
    {
        var clave = _configuracion["Jwt:Clave"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Clave.");

        var emisor = _configuracion["Jwt:Emisor"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Emisor.");

        var audiencia = _configuracion["Jwt:Audiencia"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración Jwt:Audiencia.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}"),
            new(ClaimTypes.Role, usuario.Rol.Nombre)
        };

        var llave = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(clave));

        var credenciales = new SigningCredentials(
            llave,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emisor,
            audience: audiencia,
            claims: claims,
            expires: fechaExpiracion,
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}