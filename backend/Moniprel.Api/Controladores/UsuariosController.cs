using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public UsuariosController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioRespuesta>>> Listar()
    {
        var usuarios = await _contexto.Usuarios
            .AsNoTracking()
            .Include(x => x.Rol)
            .OrderBy(x => x.Id)
            .Select(x => new UsuarioRespuesta
            {
                Id = x.Id,
                Nombres = x.Nombres,
                Apellidos = x.Apellidos,
                NombreCompleto = x.Nombres + " " + x.Apellidos,
                Correo = x.Correo,
                Activo = x.Activo,
                FechaCreacion = x.FechaCreacion,
                RolId = x.RolId,
                Rol = x.Rol.Nombre
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioRespuesta>> ObtenerPorId(int id)
    {
        var usuario = await _contexto.Usuarios
            .AsNoTracking()
            .Include(x => x.Rol)
            .Where(x => x.Id == id)
            .Select(x => new UsuarioRespuesta
            {
                Id = x.Id,
                Nombres = x.Nombres,
                Apellidos = x.Apellidos,
                NombreCompleto = x.Nombres + " " + x.Apellidos,
                Correo = x.Correo,
                Activo = x.Activo,
                FechaCreacion = x.FechaCreacion,
                RolId = x.RolId,
                Rol = x.Rol.Nombre
            })
            .SingleOrDefaultAsync();

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioRespuesta>> Crear(
        CrearUsuarioSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombres) ||
            string.IsNullOrWhiteSpace(solicitud.Apellidos) ||
            string.IsNullOrWhiteSpace(solicitud.Correo) ||
            string.IsNullOrWhiteSpace(solicitud.Contrasena))
        {
            return BadRequest(new
            {
                mensaje = "Nombres, apellidos, correo y contraseña son obligatorios."
            });
        }

        if (solicitud.Contrasena.Length < 8)
        {
            return BadRequest(new
            {
                mensaje = "La contraseña debe tener al menos 8 caracteres."
            });
        }

        var correo = solicitud.Correo.Trim().ToLowerInvariant();

        var correoExistente = await _contexto.Usuarios
            .AnyAsync(x => x.Correo == correo);

        if (correoExistente)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un usuario con ese correo electrónico."
            });
        }

        var rol = await _contexto.Roles
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.RolId &&
                x.Activo);

        if (rol is null)
        {
            return BadRequest(new
            {
                mensaje = "El rol indicado no existe o se encuentra inactivo."
            });
        }

        var usuario = new Usuario
        {
            Nombres = solicitud.Nombres.Trim(),
            Apellidos = solicitud.Apellidos.Trim(),
            Correo = correo,
            ContrasenaHash =
                BCrypt.Net.BCrypt.HashPassword(solicitud.Contrasena),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            RolId = rol.Id
        };

        _contexto.Usuarios.Add(usuario);

        await _contexto.SaveChangesAsync();

        var respuesta = new UsuarioRespuesta
        {
            Id = usuario.Id,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            NombreCompleto =
                $"{usuario.Nombres} {usuario.Apellidos}",
            Correo = usuario.Correo,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            RolId = rol.Id,
            Rol = rol.Nombre
        };

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = usuario.Id },
            respuesta);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarUsuarioSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombres) ||
            string.IsNullOrWhiteSpace(solicitud.Apellidos) ||
            string.IsNullOrWhiteSpace(solicitud.Correo))
        {
            return BadRequest(new
            {
                mensaje = "Nombres, apellidos y correo son obligatorios."
            });
        }

        var usuario = await _contexto.Usuarios
            .SingleOrDefaultAsync(x => x.Id == id);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        var correo = solicitud.Correo.Trim().ToLowerInvariant();

        var correoEnUso = await _contexto.Usuarios
            .AnyAsync(x =>
                x.Correo == correo &&
                x.Id != id);

        if (correoEnUso)
        {
            return Conflict(new
            {
                mensaje = "El correo electrónico ya está siendo utilizado por otro usuario."
            });
        }

        var rol = await _contexto.Roles
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.RolId &&
                x.Activo);

        if (rol is null)
        {
            return BadRequest(new
            {
                mensaje = "El rol indicado no existe o se encuentra inactivo."
            });
        }

        usuario.Nombres = solicitud.Nombres.Trim();
        usuario.Apellidos = solicitud.Apellidos.Trim();
        usuario.Correo = correo;
        usuario.RolId = rol.Id;

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Usuario actualizado correctamente."
        });
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        CambiarEstadoUsuarioSolicitud solicitud)
    {
        var usuario = await _contexto.Usuarios
            .SingleOrDefaultAsync(x => x.Id == id);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        usuario.Activo = solicitud.Activo;

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = solicitud.Activo
                ? "Usuario activado correctamente."
                : "Usuario desactivado correctamente."
        });
    }

    [HttpPatch("{id:int}/contrasena")]
    public async Task<IActionResult> CambiarContrasena(
        int id,
        CambiarContrasenaUsuarioSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.NuevaContrasena) ||
            solicitud.NuevaContrasena.Length < 8)
        {
            return BadRequest(new
            {
                mensaje = "La nueva contraseña debe tener al menos 8 caracteres."
            });
        }

        var usuario = await _contexto.Usuarios
            .SingleOrDefaultAsync(x => x.Id == id);

        if (usuario is null)
        {
            return NotFound(new
            {
                mensaje = "El usuario no existe."
            });
        }

        usuario.ContrasenaHash =
            BCrypt.Net.BCrypt.HashPassword(solicitud.NuevaContrasena);

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Contraseña actualizada correctamente."
        });
    }
}