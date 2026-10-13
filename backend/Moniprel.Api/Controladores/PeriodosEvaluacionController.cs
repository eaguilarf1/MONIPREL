using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/periodos-evaluacion")]
[Authorize(Roles = "Administrador")]
public class PeriodosEvaluacionController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public PeriodosEvaluacionController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var periodos = await _contexto.PeriodosEvaluacion
            .AsNoTracking()
            .Include(x => x.Instrumento)
            .OrderByDescending(x => x.FechaInicio)
            .Select(x => new
            {
                x.Id,
                x.InstrumentoId,
                Instrumento = x.Instrumento.Nombre,
                x.Nombre,
                x.FechaInicio,
                x.FechaFin,
                x.Activo,
                x.FechaCreacion
            })
            .ToListAsync();

        return Ok(periodos);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearPeriodoEvaluacionSolicitud solicitud)
    {
        if (solicitud.InstrumentoId <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El instrumento es obligatorio."
            });
        }

        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre del período es obligatorio."
            });
        }

        var fechaInicioUtc = solicitud.FechaInicio.ToUniversalTime();
        var fechaFinUtc = solicitud.FechaFin.ToUniversalTime();

        if (fechaInicioUtc >= fechaFinUtc)
        {
            return BadRequest(new
            {
                mensaje = "La fecha de inicio debe ser anterior a la fecha de fin."
            });
        }

        var instrumento = await _contexto.Instrumentos
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.InstrumentoId &&
                x.Activo);

        if (instrumento is null)
        {
            return BadRequest(new
            {
                mensaje = "El instrumento no existe o se encuentra inactivo."
            });
        }

        var existeSolapamiento = await _contexto.PeriodosEvaluacion
            .AnyAsync(x =>
                x.InstrumentoId == solicitud.InstrumentoId &&
                x.Activo &&
                fechaInicioUtc < x.FechaFin &&
                fechaFinUtc > x.FechaInicio);

        if (existeSolapamiento)
        {
            return Conflict(new
            {
                mensaje = "Ya existe un período activo que se superpone con las fechas indicadas."
            });
        }

        var periodo = new PeriodoEvaluacion
        {
            InstrumentoId = solicitud.InstrumentoId,
            Nombre = solicitud.Nombre.Trim(),
            FechaInicio = fechaInicioUtc,
            FechaFin = fechaFinUtc,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _contexto.PeriodosEvaluacion.Add(periodo);

        await _contexto.SaveChangesAsync();

        return Created(
            $"/api/periodos-evaluacion/{periodo.Id}",
            new
            {
                periodo.Id,
                periodo.InstrumentoId,
                periodo.Nombre,
                periodo.FechaInicio,
                periodo.FechaFin,
                periodo.Activo,
                periodo.FechaCreacion
            });
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int id,
        CambiarEstadoPeriodoSolicitud solicitud)
    {
        var periodo = await _contexto.PeriodosEvaluacion
            .SingleOrDefaultAsync(x => x.Id == id);

        if (periodo is null)
        {
            return NotFound(new
            {
                mensaje = "El período de evaluación no existe."
            });
        }

        periodo.Activo = solicitud.Activo;

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = solicitud.Activo
                ? "Período activado correctamente."
                : "Período desactivado correctamente."
        });
    }
}