using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/evaluaciones")]
[Authorize(Roles = "Colaborador")]
public class EvaluacionesController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public EvaluacionesController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<EvaluacionDisponibleRespuesta>>>
        ObtenerDisponibles()
    {
        var identificadorUsuario =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificadorUsuario, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var usuario = await _contexto.Usuarios
            .AsNoTracking()
            .Include(x => x.Rol)
            .SingleOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario is null || !usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "El usuario autenticado no se encuentra activo."
            });
        }

        if (usuario.Rol.Nombre != "Colaborador")
        {
            return Forbid();
        }

        var fechaActual = DateTime.UtcNow;

        var periodos = await _contexto.PeriodosEvaluacion
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.Instrumento.Activo &&
                x.FechaInicio <= fechaActual &&
                x.FechaFin >= fechaActual &&
                !_contexto.Evaluaciones.Any(e =>
                    e.UsuarioId == usuarioId &&
                    e.PeriodoEvaluacionId == x.Id))
            .OrderBy(x => x.FechaFin)
            .Select(x => new EvaluacionDisponibleRespuesta
            {
                PeriodoEvaluacionId = x.Id,
                Periodo = x.Nombre,
                FechaInicio = x.FechaInicio,
                FechaFin = x.FechaFin,

                InstrumentoId = x.Instrumento.Id,
                Instrumento = x.Instrumento.Nombre,
                DescripcionInstrumento = x.Instrumento.Descripcion,

                Dimensiones = x.Instrumento.Dimensiones
                    .Where(d => d.Activa)
                    .OrderBy(d => d.Orden)
                    .Select(d => new DimensionDisponibleRespuesta
                    {
                        Id = d.Id,
                        Nombre = d.Nombre,
                        Descripcion = d.Descripcion,
                        Orden = d.Orden,

                        Preguntas = d.Preguntas
                            .Where(p => p.Activa)
                            .OrderBy(p => p.Orden)
                            .Select(p => new PreguntaDisponibleRespuesta
                            {
                                Id = p.Id,
                                Texto = p.Texto,
                                Orden = p.Orden
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(periodos);
    }

    [HttpPost]
    public async Task<IActionResult> Realizar(
        RealizarEvaluacionSolicitud solicitud)
    {
        var identificadorUsuario =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificadorUsuario, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var usuario = await _contexto.Usuarios
            .AsNoTracking()
            .Include(x => x.Rol)
            .SingleOrDefaultAsync(x => x.Id == usuarioId);

        if (usuario is null || !usuario.Activo)
        {
            return Unauthorized(new
            {
                mensaje = "El usuario autenticado no se encuentra activo."
            });
        }

        var fechaActual = DateTime.UtcNow;

        var periodo = await _contexto.PeriodosEvaluacion
            .Include(x => x.Instrumento)
                .ThenInclude(x => x.Dimensiones)
                    .ThenInclude(x => x.Preguntas)
            .SingleOrDefaultAsync(x =>
                x.Id == solicitud.PeriodoEvaluacionId);

        if (periodo is null)
        {
            return NotFound(new
            {
                mensaje = "El período de evaluación no existe."
            });
        }

        if (!periodo.Activo ||
            !periodo.Instrumento.Activo ||
            fechaActual < periodo.FechaInicio ||
            fechaActual > periodo.FechaFin)
        {
            return BadRequest(new
            {
                mensaje = "El período de evaluación no se encuentra disponible."
            });
        }

        var evaluacionExistente = await _contexto.Evaluaciones
            .AnyAsync(x =>
                x.UsuarioId == usuarioId &&
                x.PeriodoEvaluacionId == periodo.Id);

        if (evaluacionExistente)
        {
            return Conflict(new
            {
                mensaje = "Ya existe una evaluación registrada para este período."
            });
        }

        var preguntasActivas = periodo.Instrumento.Dimensiones
            .Where(x => x.Activa)
            .SelectMany(x => x.Preguntas)
            .Where(x => x.Activa)
            .ToList();

        if (solicitud.Respuestas.Count != preguntasActivas.Count)
        {
            return BadRequest(new
            {
                mensaje =
                    $"Debe responder las {preguntasActivas.Count} preguntas del instrumento."
            });
        }

        var preguntasDuplicadas = solicitud.Respuestas
            .GroupBy(x => x.PreguntaId)
            .Any(x => x.Count() > 1);

        if (preguntasDuplicadas)
        {
            return BadRequest(new
            {
                mensaje = "No se puede responder una misma pregunta más de una vez."
            });
        }

        var idsPreguntasActivas = preguntasActivas
            .Select(x => x.Id)
            .ToHashSet();

        var contienePreguntasInvalidas = solicitud.Respuestas
            .Any(x => !idsPreguntasActivas.Contains(x.PreguntaId));

        if (contienePreguntasInvalidas)
        {
            return BadRequest(new
            {
                mensaje = "Una o más preguntas no pertenecen al instrumento activo."
            });
        }

        var contieneValoresInvalidos = solicitud.Respuestas
            .Any(x => x.Valor < 1 || x.Valor > 5);

        if (contieneValoresInvalidos)
        {
            return BadRequest(new
            {
                mensaje = "Todas las respuestas deben tener un valor entre 1 y 5."
            });
        }

        var evaluacion = new Evaluacion
        {
            UsuarioId = usuarioId,
            PeriodoEvaluacionId = periodo.Id,
            FechaInicio = fechaActual,
            FechaFinalizacion = fechaActual,
            Finalizada = true
        };

        foreach (var respuestaSolicitud in solicitud.Respuestas)
        {
            evaluacion.Respuestas.Add(new Respuesta
            {
                PreguntaId = respuestaSolicitud.PreguntaId,
                Valor = respuestaSolicitud.Valor
            });
        }

        evaluacion.Resultado = ConstruirResultado(
            preguntasActivas,
            solicitud.Respuestas);

        _contexto.Evaluaciones.Add(evaluacion);

        await _contexto.SaveChangesAsync();

        return Created(
            $"/api/evaluaciones/{evaluacion.Id}/resultado",
            new
            {
                evaluacion.Id,
                evaluacion.PeriodoEvaluacionId,
                evaluacion.FechaFinalizacion,
                evaluacion.Finalizada,
                CantidadRespuestas = evaluacion.Respuestas.Count,
                PuntajeGeneral = evaluacion.Resultado.PuntajeGeneral,
                NivelRiesgo = evaluacion.Resultado.NivelRiesgo,
                mensaje = "Evaluación registrada y procesada correctamente."
            });
    }

    [HttpGet("{id:int}/resultado")]
    public async Task<ActionResult<ResultadoEvaluacionRespuesta>>
        ObtenerResultado(int id)
    {
        var identificadorUsuario =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificadorUsuario, out var usuarioId))
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var evaluacion = await _contexto.Evaluaciones
            .Include(x => x.Resultado)
                .ThenInclude(x => x!.ResultadosDimension)
                    .ThenInclude(x => x.Dimension)
            .Include(x => x.Respuestas)
                .ThenInclude(x => x.Pregunta)
                    .ThenInclude(x => x.Dimension)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (evaluacion is null)
        {
            return NotFound(new
            {
                mensaje = "La evaluación no existe."
            });
        }

        if (evaluacion.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        if (!evaluacion.Finalizada)
        {
            return BadRequest(new
            {
                mensaje = "La evaluación aún no ha sido finalizada."
            });
        }

        if (evaluacion.Resultado is null)
        {
            evaluacion.Resultado =
                ConstruirResultadoDesdeEvaluacion(evaluacion);

            await _contexto.SaveChangesAsync();
        }

        var resultado = evaluacion.Resultado;

        var respuesta = new ResultadoEvaluacionRespuesta
        {
            EvaluacionId = evaluacion.Id,
            PeriodoEvaluacionId = evaluacion.PeriodoEvaluacionId,
            PuntajeGeneral = resultado.PuntajeGeneral,
            NivelRiesgo = resultado.NivelRiesgo,
            FechaCalculo = resultado.FechaCalculo,

            Dimensiones = resultado.ResultadosDimension
                .OrderBy(x => x.Dimension.Id)
                .Select(x => new ResultadoDimensionRespuesta
                {
                    DimensionId = x.DimensionId,
                    Dimension = x.Dimension.Nombre,
                    Puntaje = x.Puntaje
                })
                .ToList()
        };

        return Ok(respuesta);
    }

    private static Resultado ConstruirResultado(
        List<Pregunta> preguntas,
        List<RespuestaEvaluacionSolicitud> respuestas)
    {
        var valoresPorDimension = new Dictionary<int, List<decimal>>();

        foreach (var respuesta in respuestas)
        {
            var pregunta = preguntas.Single(x =>
                x.Id == respuesta.PreguntaId);

            var valorRiesgo = pregunta.EsProtectora
                ? 6 - respuesta.Valor
                : respuesta.Valor;

            if (!valoresPorDimension.ContainsKey(pregunta.DimensionId))
            {
                valoresPorDimension[pregunta.DimensionId] =
                    new List<decimal>();
            }

            valoresPorDimension[pregunta.DimensionId]
                .Add(valorRiesgo);
        }

        return CrearResultado(valoresPorDimension);
    }

    private static Resultado ConstruirResultadoDesdeEvaluacion(
        Evaluacion evaluacion)
    {
        var valoresPorDimension = new Dictionary<int, List<decimal>>();

        foreach (var respuesta in evaluacion.Respuestas)
        {
            var pregunta = respuesta.Pregunta;

            var valorRiesgo = pregunta.EsProtectora
                ? 6 - respuesta.Valor
                : respuesta.Valor;

            if (!valoresPorDimension.ContainsKey(pregunta.DimensionId))
            {
                valoresPorDimension[pregunta.DimensionId] =
                    new List<decimal>();
            }

            valoresPorDimension[pregunta.DimensionId]
                .Add(valorRiesgo);
        }

        return CrearResultado(valoresPorDimension);
    }

    private static Resultado CrearResultado(
        Dictionary<int, List<decimal>> valoresPorDimension)
    {
        var resultado = new Resultado
        {
            FechaCalculo = DateTime.UtcNow
        };

        foreach (var dimension in valoresPorDimension)
        {
            var promedio = Math.Round(
                dimension.Value.Average(),
                2,
                MidpointRounding.AwayFromZero);

            resultado.ResultadosDimension.Add(
                new ResultadoDimension
                {
                    DimensionId = dimension.Key,
                    Puntaje = promedio
                });
        }

        var puntajeGeneral = Math.Round(
            resultado.ResultadosDimension
                .Average(x => x.Puntaje),
            2,
            MidpointRounding.AwayFromZero);

        resultado.PuntajeGeneral = puntajeGeneral;
        resultado.NivelRiesgo =
            ClasificarNivelRiesgo(puntajeGeneral);

        return resultado;
    }

    private static string ClasificarNivelRiesgo(
        decimal puntajeGeneral)
    {
        if (puntajeGeneral <= 2.33m)
        {
            return "Bajo";
        }

        if (puntajeGeneral <= 3.66m)
        {
            return "Moderado";
        }

        return "Alto";
    }
}