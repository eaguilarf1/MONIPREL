using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/evaluaciones")]
[Authorize(Roles = "Colaborador")]
public class ConsultasEvaluacionController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public ConsultasEvaluacionController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet("{id:int}/recomendaciones")]
    public async Task<ActionResult<RecomendacionesEvaluacionRespuesta>>
        ObtenerRecomendaciones(int id)
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var evaluacion = await _contexto.Evaluaciones
            .AsNoTracking()
            .Include(x => x.Resultado)
                .ThenInclude(x => x!.ResultadosDimension)
                    .ThenInclude(x => x.Dimension)
            .SingleOrDefaultAsync(x =>
                x.Id == id &&
                x.UsuarioId == usuarioId.Value);

        if (evaluacion is null)
        {
            return NotFound(new
            {
                mensaje = "La evaluación no existe o no pertenece al usuario autenticado."
            });
        }

        if (!evaluacion.Finalizada)
        {
            return BadRequest(new
            {
                mensaje = "La evaluación todavía no ha sido finalizada."
            });
        }

        if (evaluacion.Resultado is null)
        {
            return BadRequest(new
            {
                mensaje = "La evaluación todavía no cuenta con un resultado calculado."
            });
        }

        var puntajesDimension = evaluacion.Resultado.ResultadosDimension
            .Select(x => new PuntajeDimension
            {
                Nombre = x.Dimension.Nombre,
                Puntaje = x.Puntaje
            })
            .ToList();

        var respuesta = new RecomendacionesEvaluacionRespuesta
        {
            EvaluacionId = evaluacion.Id,
            PuntajeGeneral = evaluacion.Resultado.PuntajeGeneral,
            NivelRiesgo = evaluacion.Resultado.NivelRiesgo,
            Recomendaciones = GenerarRecomendaciones(
                evaluacion.Resultado.NivelRiesgo,
                puntajesDimension)
        };

        return Ok(respuesta);
    }

    [HttpGet("historial")]
    public async Task<ActionResult<IEnumerable<HistorialEvaluacionRespuesta>>>
        ObtenerHistorial()
    {
        var usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var historial = await _contexto.Evaluaciones
            .AsNoTracking()
            .Where(x =>
                x.UsuarioId == usuarioId.Value &&
                x.Finalizada &&
                x.Resultado != null)
            .OrderByDescending(x => x.FechaFinalizacion)
            .Select(x => new HistorialEvaluacionRespuesta
            {
                EvaluacionId = x.Id,
                PeriodoEvaluacionId = x.PeriodoEvaluacionId,
                Periodo = x.PeriodoEvaluacion.Nombre,
                FechaInicioPeriodo = x.PeriodoEvaluacion.FechaInicio,
                FechaFinPeriodo = x.PeriodoEvaluacion.FechaFin,
                FechaFinalizacion = x.FechaFinalizacion,

                PuntajeGeneral = x.Resultado!.PuntajeGeneral,
                NivelRiesgo = x.Resultado.NivelRiesgo,

                Dimensiones = x.Resultado.ResultadosDimension
                    .OrderBy(d => d.Dimension.Id)
                    .Select(d => new HistorialDimensionRespuesta
                    {
                        DimensionId = d.DimensionId,
                        Dimension = d.Dimension.Nombre,
                        Puntaje = d.Puntaje
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(historial);
    }

    private int? ObtenerUsuarioId()
    {
        var identificadorUsuario =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificadorUsuario, out var usuarioId))
        {
            return null;
        }

        return usuarioId;
    }

    private static List<RecomendacionRespuesta> GenerarRecomendaciones(
        string nivelRiesgo,
        IEnumerable<PuntajeDimension> resultadosDimension)
    {
        var recomendaciones = new List<RecomendacionRespuesta>();

        recomendaciones.Add(
            ObtenerRecomendacionGeneral(nivelRiesgo));

        foreach (var resultado in resultadosDimension)
        {
            if (resultado.Puntaje < 2.34m)
            {
                continue;
            }

            var recomendacion =
                ObtenerRecomendacionDimension(
                    resultado.Nombre,
                    resultado.Puntaje);

            if (recomendacion is not null)
            {
                recomendaciones.Add(recomendacion);
            }
        }

        return recomendaciones;
    }

    private static RecomendacionRespuesta ObtenerRecomendacionGeneral(
        string nivelRiesgo)
    {
        return nivelRiesgo switch
        {
            "Bajo" => new RecomendacionRespuesta
            {
                Tipo = "General",
                Titulo = "Mantener prácticas laborales favorables",
                Descripcion =
                    "El resultado general se encuentra en un nivel bajo. " +
                    "Se recomienda mantener las prácticas actuales de organización, " +
                    "comunicación, apoyo y recuperación, y continuar participando " +
                    "en las evaluaciones periódicas."
            },

            "Moderado" => new RecomendacionRespuesta
            {
                Tipo = "General",
                Titulo = "Fortalecer medidas preventivas",
                Descripcion =
                    "El resultado general se encuentra en un nivel moderado. " +
                    "Se recomienda revisar preventivamente los factores laborales " +
                    "con mayor puntuación, organizar prioridades, utilizar los recursos " +
                    "de apoyo disponibles y mantener seguimiento periódico."
            },

            _ => new RecomendacionRespuesta
            {
                Tipo = "General",
                Titulo = "Priorizar acciones preventivas",
                Descripcion =
                    "El resultado general se encuentra en un nivel alto. " +
                    "Se recomienda priorizar la revisión de las condiciones laborales " +
                    "con mayor puntuación, utilizar los canales de apoyo disponibles " +
                    "y dar seguimiento oportuno a las medidas preventivas. " +
                    "Este resultado constituye una orientación preventiva y no un diagnóstico."
            }
        };
    }

    private static RecomendacionRespuesta? ObtenerRecomendacionDimension(
        string dimension,
        decimal puntaje)
    {
        var intensidad = puntaje >= 3.67m
            ? "Se recomienda priorizar"
            : "Se recomienda considerar";

        return dimension switch
        {
            "Carga laboral percibida" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Organización de la carga laboral",
                    $"{intensidad} la revisión de la distribución de tareas, " +
                    "los tiempos disponibles y las prioridades de trabajo."),

            "Apoyo social laboral percibido" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Fortalecimiento del apoyo laboral",
                    $"{intensidad} el uso de canales de comunicación y apoyo " +
                    "dentro del equipo ante dudas, dificultades o períodos de alta carga."),

            "Control/autonomía percibida" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Organización y autonomía",
                    $"{intensidad} prácticas que permitan organizar tareas, " +
                    "establecer prioridades y participar en mejoras relacionadas con el trabajo."),

            "Claridad de rol y prioridades" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Claridad de funciones y prioridades",
                    $"{intensidad} la revisión de expectativas, responsabilidades " +
                    "y prioridades para reducir cambios o ambigüedades innecesarias."),

            "Recompensa percibida" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Reconocimiento y desarrollo",
                    $"{intensidad} los mecanismos de retroalimentación, reconocimiento " +
                    "y claridad sobre oportunidades de desarrollo laboral."),

            "Señales asociadas al nivel de riesgo" =>
                CrearRecomendacionDimension(
                    dimension,
                    "Recuperación y seguimiento preventivo",
                    $"{intensidad} medidas de recuperación después de la jornada, " +
                    "organización de pausas y seguimiento preventivo de las señales percibidas."),

            _ => null
        };
    }

    private static RecomendacionRespuesta CrearRecomendacionDimension(
        string dimension,
        string titulo,
        string descripcion)
    {
        return new RecomendacionRespuesta
        {
            Tipo = "Dimensión",
            Dimension = dimension,
            Titulo = titulo,
            Descripcion = descripcion
        };
    }

    private class PuntajeDimension
    {
        public string Nombre { get; set; } = string.Empty;

        public decimal Puntaje { get; set; }
    }
}