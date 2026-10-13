using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/instrumentos")]
[Authorize(Roles = "Administrador")]
public class InstrumentosController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public InstrumentosController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var instrumentos = await _contexto.Instrumentos
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Activo,
                x.FechaCreacion
            })
            .ToListAsync();

        return Ok(instrumentos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var instrumento = await _contexto.Instrumentos
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Activo,
                x.FechaCreacion,
                Dimensiones = x.Dimensiones
                    .OrderBy(d => d.Orden)
                    .Select(d => new
                    {
                        d.Id,
                        d.Nombre,
                        d.Descripcion,
                        d.Orden,
                        d.Activa,
                        Preguntas = d.Preguntas
                            .OrderBy(p => p.Orden)
                            .Select(p => new
                            {
                                p.Id,
                                p.Texto,
                                p.Orden,
                                p.EsProtectora,
                                p.Activa
                            })
                    })
            })
            .SingleOrDefaultAsync();

        if (instrumento is null)
        {
            return NotFound(new
            {
                mensaje = "El instrumento no existe."
            });
        }

        return Ok(instrumento);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        CrearInstrumentoSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre del instrumento es obligatorio."
            });
        }

        var instrumento = new Instrumento
        {
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = solicitud.Descripcion?.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        _contexto.Instrumentos.Add(instrumento);

        await _contexto.SaveChangesAsync();

        return Created(
            $"/api/instrumentos/{instrumento.Id}",
            new
            {
                instrumento.Id,
                instrumento.Nombre,
                instrumento.Descripcion,
                instrumento.Activo,
                instrumento.FechaCreacion
            });
    }

    [HttpPost("{instrumentoId:int}/dimensiones")]
    public async Task<IActionResult> CrearDimension(
        int instrumentoId,
        CrearDimensionSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            return BadRequest(new
            {
                mensaje = "El nombre de la dimensión es obligatorio."
            });
        }

        if (solicitud.Orden <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El orden debe ser mayor que cero."
            });
        }

        var instrumento = await _contexto.Instrumentos
            .SingleOrDefaultAsync(x =>
                x.Id == instrumentoId &&
                x.Activo);

        if (instrumento is null)
        {
            return BadRequest(new
            {
                mensaje = "El instrumento no existe o se encuentra inactivo."
            });
        }

        var ordenExistente = await _contexto.Dimensiones
            .AnyAsync(x =>
                x.InstrumentoId == instrumentoId &&
                x.Orden == solicitud.Orden);

        if (ordenExistente)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una dimensión con ese orden dentro del instrumento."
            });
        }

        var dimension = new Dimension
        {
            InstrumentoId = instrumentoId,
            Nombre = solicitud.Nombre.Trim(),
            Descripcion = solicitud.Descripcion?.Trim(),
            Orden = solicitud.Orden,
            Activa = true
        };

        _contexto.Dimensiones.Add(dimension);

        await _contexto.SaveChangesAsync();

        return Created(
            $"/api/instrumentos/{instrumentoId}",
            new
            {
                dimension.Id,
                dimension.InstrumentoId,
                dimension.Nombre,
                dimension.Descripcion,
                dimension.Orden,
                dimension.Activa
            });
    }

    [HttpPost("dimensiones/{dimensionId:int}/preguntas")]
    public async Task<IActionResult> CrearPregunta(
        int dimensionId,
        CrearPreguntaSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Texto))
        {
            return BadRequest(new
            {
                mensaje = "El texto de la pregunta es obligatorio."
            });
        }

        if (solicitud.Orden <= 0)
        {
            return BadRequest(new
            {
                mensaje = "El orden debe ser mayor que cero."
            });
        }

        var dimension = await _contexto.Dimensiones
            .SingleOrDefaultAsync(x =>
                x.Id == dimensionId &&
                x.Activa);

        if (dimension is null)
        {
            return BadRequest(new
            {
                mensaje = "La dimensión no existe o se encuentra inactiva."
            });
        }

        var ordenExistente = await _contexto.Preguntas
            .AnyAsync(x =>
                x.DimensionId == dimensionId &&
                x.Orden == solicitud.Orden);

        if (ordenExistente)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una pregunta con ese orden dentro de la dimensión."
            });
        }

        var pregunta = new Pregunta
        {
            DimensionId = dimensionId,
            Texto = solicitud.Texto.Trim(),
            Orden = solicitud.Orden,
            EsProtectora = solicitud.EsProtectora,
            Activa = true
        };

        _contexto.Preguntas.Add(pregunta);

        await _contexto.SaveChangesAsync();

        return Created(
            $"/api/instrumentos/dimensiones/{dimensionId}/preguntas/{pregunta.Id}",
            new
            {
                pregunta.Id,
                pregunta.DimensionId,
                pregunta.Texto,
                pregunta.Orden,
                pregunta.EsProtectora,
                pregunta.Activa
            });
    }
}