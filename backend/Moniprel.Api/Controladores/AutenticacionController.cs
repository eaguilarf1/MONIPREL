using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Interfaces;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/autenticacion")]
public class AutenticacionController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;
    private readonly IServicioToken _servicioToken;

    public AutenticacionController(
        ContextoMoniprel contexto,
        IServicioToken servicioToken)
    {
        _contexto = contexto;
        _servicioToken = servicioToken;
    }

    [HttpPost("iniciar-sesion")]
    public async Task<ActionResult<InicioSesionRespuesta>> IniciarSesion(
        InicioSesionSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Correo) ||
            string.IsNullOrWhiteSpace(solicitud.Contrasena))
        {
            return BadRequest(new
            {
                mensaje = "Correo y contraseña son obligatorios."
            });
        }

        var correo = solicitud.Correo.Trim().ToLowerInvariant();

        var usuario = await _contexto.Usuarios
            .Include(x => x.Rol)
            .SingleOrDefaultAsync(x => x.Correo == correo);

        if (usuario is null ||
            !usuario.Activo ||
            !BCrypt.Net.BCrypt.Verify(
                solicitud.Contrasena,
                usuario.ContrasenaHash))
        {
            return Unauthorized(new
            {
                mensaje = "Credenciales inválidas."
            });
        }

        if (!usuario.Rol.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "El rol asignado al usuario se encuentra inactivo."
            });
        }

        var expiracion = DateTime.UtcNow.AddHours(2);

        var token = _servicioToken.GenerarToken(
            usuario,
            expiracion);

        return Ok(new InicioSesionRespuesta
        {
            Token = token,
            ExpiraEn = expiracion,
            UsuarioId = usuario.Id,
            NombreCompleto =
                $"{usuario.Nombres} {usuario.Apellidos}",
            Correo = usuario.Correo,
            Rol = usuario.Rol.Nombre
        });
    }
}