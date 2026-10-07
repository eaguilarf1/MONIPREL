using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/asignaciones")]
[Authorize(Roles = "Administrador")]
public class AsignacionesController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public AsignacionesController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AsignacionRespuesta>>> Listar()
    {
        var asignaciones = await _contexto.AsignacionesOrganizacionales
            .AsNoTracking()
            .Include(x => x.Supervisor)
            .Include(x => x.Colaborador)
            .OrderBy(x => x.Id)
            .Select(x => new AsignacionRespuesta
            {
                Id = x.Id,
                SupervisorId = x.SupervisorId,
                Supervisor = x.Supervisor.Nombres + " " + x.Supervisor.Apellidos,
                ColaboradorId = x.ColaboradorId,
                Colaborador = x.Colaborador.Nombres + " " + x.Colaborador.Apellidos,
                Activa = x.Activa,
                FechaCreacion = x.FechaCreacion
            })
            .ToListAsync();

        return Ok(asignaciones);
    }

    [HttpPost]
    public async Task<ActionResult<AsignacionRespuesta>> Crear(
        CrearAsignacionSolicitud solicitud)
    {
        if (solicitud.SupervisorId <= 0 ||
            solicitud.ColaboradorId <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El supervisor y el colaborador son obligatorios."
            });
        }

        if (solicitud.SupervisorId == solicitud.ColaboradorId)
        {
            return BadRequest(new
            {
                mensaje = "Un usuario no puede ser supervisor y colaborador de sí mismo."
            });
        }

        var supervisor = await _contexto.Usuarios
            .Include(x => x.Rol)
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.SupervisorId);

        if (supervisor is null)
        {
            return BadRequest(new
            {
                mensaje = "El supervisor indicado no existe."
            });
        }

        if (!supervisor.Activo)
        {
            return BadRequest(new
            {
                mensaje = "El supervisor se encuentra inactivo."
            });
        }

        if (supervisor.Rol.Nombre != "Supervisor")
        {
            return BadRequest(new
            {
                mensaje = "El usuario indicado no posee el rol Supervisor."
            });
        }

        var colaborador = await _contexto.Usuarios
            .Include(x => x.Rol)
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.ColaboradorId);

        if (colaborador is null)
        {
            return BadRequest(new
            {
                mensaje = "El colaborador indicado no existe."
            });
        }

        if (!colaborador.Activo)
        {
            return BadRequest(new
            {
                mensaje = "El colaborador se encuentra inactivo."
            });
        }

        if (colaborador.Rol.Nombre != "Colaborador")
        {
            return BadRequest(new
            {
                mensaje = "El usuario indicado no posee el rol Colaborador."
            });
        }

        var asignacionExistente =
            await _contexto.AsignacionesOrganizacionales
                .SingleOrDefaultAsync(x =>
                    x.SupervisorId == solicitud.SupervisorId &&
                    x.ColaboradorId == solicitud.ColaboradorId);

        if (asignacionExistente is not null)
        {
            if (asignacionExistente.Activa)
            {
                return Conflict(new
                {
                    mensaje = "El colaborador ya se encuentra asignado a este supervisor."
                });
            }

            asignacionExistente.Activa = true;

            await _contexto.SaveChangesAsync();

            return Ok(new AsignacionRespuesta
            {
                Id = asignacionExistente.Id,
                SupervisorId = supervisor.Id,
                Supervisor =
                    $"{supervisor.Nombres} {supervisor.Apellidos}",
                ColaboradorId = colaborador.Id,
                Colaborador =
                    $"{colaborador.Nombres} {colaborador.Apellidos}",
                Activa = true,
                FechaCreacion = asignacionExistente.FechaCreacion
            });
        }

        var asignacion = new AsignacionOrganizacional
        {
            SupervisorId = supervisor.Id,
            ColaboradorId = colaborador.Id,
            Activa = true,
            FechaCreacion = DateTime.UtcNow
        };

        _contexto.AsignacionesOrganizacionales.Add(asignacion);

        await _contexto.SaveChangesAsync();

        var respuesta = new AsignacionRespuesta
        {
            Id = asignacion.Id,
            SupervisorId = supervisor.Id,
            Supervisor =
                $"{supervisor.Nombres} {supervisor.Apellidos}",
            ColaboradorId = colaborador.Id,
            Colaborador =
                $"{colaborador.Nombres} {colaborador.Apellidos}",
            Activa = asignacion.Activa,
            FechaCreacion = asignacion.FechaCreacion
        };

        return Created(
            $"/api/asignaciones/{asignacion.Id}",
            respuesta);
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        CambiarEstadoAsignacionSolicitud solicitud)
    {
        var asignacion =
            await _contexto.AsignacionesOrganizacionales
                .SingleOrDefaultAsync(x => x.Id == id);

        if (asignacion is null)
        {
            return NotFound(new
            {
                mensaje = "La asignación no existe."
            });
        }

        asignacion.Activa = solicitud.Activa;

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = solicitud.Activa
                ? "Asignación activada correctamente."
                : "Asignación desactivada correctamente."
        });
    }
}