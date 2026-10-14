using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Interfaces;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Administrador")]
public class ReportesController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;
    private readonly IServicioCorreo _servicioCorreo;

    public ReportesController(
        ContextoMoniprel contexto,
        IServicioCorreo servicioCorreo)
    {
        _contexto = contexto;
        _servicioCorreo = servicioCorreo;
    }

    [HttpGet("periodos/{periodoId:int}/consolidado")]
    public async Task<ActionResult<ReporteConsolidadoRespuesta>>
        ObtenerConsolidado(int periodoId)
    {
        var reporte = await ConstruirReporteConsolidado(periodoId);

        if (reporte is null)
        {
            return NotFound(new
            {
                mensaje = "El período de evaluación no existe."
            });
        }

        return Ok(reporte);
    }

    [HttpPost("periodos/{periodoId:int}/enviar")]
    public async Task<IActionResult> EnviarReporte(int periodoId)
    {
        var configuracion = await _contexto.ConfiguracionesReporte
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (configuracion is null)
        {
            return BadRequest(new
            {
                mensaje =
                    "No existe una configuración de reportes. " +
                    "Configure primero el correo destinatario."
            });
        }

        if (string.IsNullOrWhiteSpace(
            configuracion.CorreoDestinatario))
        {
            return BadRequest(new
            {
                mensaje =
                    "No existe un correo destinatario configurado."
            });
        }

        var reporte = await ConstruirReporteConsolidado(periodoId);

        if (reporte is null)
        {
            return NotFound(new
            {
                mensaje = "El período de evaluación no existe."
            });
        }

        var asunto =
            $"MONIPREL - Reporte consolidado - {reporte.Periodo}";

        var cuerpoHtml = ConstruirCorreoHtml(reporte);

        try
        {
            await _servicioCorreo.EnviarAsync(
                configuracion.CorreoDestinatario,
                asunto,
                cuerpoHtml);
        }
        catch (Exception ex)
{
    return StatusCode(
        StatusCodes.Status500InternalServerError,
        new
        {
            mensaje = "No fue posible enviar el reporte por correo.",
            detalle = ex.Message,
            interno = ex.InnerException?.Message
        });
}

        configuracion.UltimoEnvio = DateTime.UtcNow;

        await _contexto.SaveChangesAsync();

        return Ok(new
        {
            mensaje = "Reporte enviado correctamente.",
            destinatario = configuracion.CorreoDestinatario,
            periodoEvaluacionId = reporte.PeriodoEvaluacionId,
            periodo = reporte.Periodo,
            ultimoEnvio = configuracion.UltimoEnvio
        });
    }

    private async Task<ReporteConsolidadoRespuesta?>
        ConstruirReporteConsolidado(int periodoId)
    {
        var periodo = await _contexto.PeriodosEvaluacion
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == periodoId);

        if (periodo is null)
        {
            return null;
        }

        var resultados = await _contexto.Resultados
            .AsNoTracking()
            .Where(x =>
                x.Evaluacion.PeriodoEvaluacionId == periodoId &&
                x.Evaluacion.Finalizada)
            .ToListAsync();

        if (resultados.Count == 0)
        {
            return new ReporteConsolidadoRespuesta
            {
                PeriodoEvaluacionId = periodo.Id,
                Periodo = periodo.Nombre,
                FechaInicio = periodo.FechaInicio,
                FechaFin = periodo.FechaFin,
                TotalEvaluaciones = 0,
                TotalBajo = 0,
                TotalModerado = 0,
                TotalAlto = 0,
                PuntajeGeneralPromedio = 0,
                Dimensiones = new List<ReporteDimensionRespuesta>()
            };
        }

        var dimensiones = await _contexto.ResultadosDimension
            .AsNoTracking()
            .Where(x =>
                x.Resultado.Evaluacion.PeriodoEvaluacionId ==
                    periodoId &&
                x.Resultado.Evaluacion.Finalizada)
            .GroupBy(x => new
            {
                x.DimensionId,
                x.Dimension.Nombre
            })
            .Select(x => new ReporteDimensionRespuesta
            {
                DimensionId = x.Key.DimensionId,
                Dimension = x.Key.Nombre,

                PuntajePromedio = Math.Round(
                    x.Average(y => y.Puntaje),
                    2)
            })
            .OrderBy(x => x.DimensionId)
            .ToListAsync();

        return new ReporteConsolidadoRespuesta
        {
            PeriodoEvaluacionId = periodo.Id,
            Periodo = periodo.Nombre,
            FechaInicio = periodo.FechaInicio,
            FechaFin = periodo.FechaFin,

            TotalEvaluaciones = resultados.Count,

            TotalBajo = resultados.Count(x =>
                x.NivelRiesgo == "Bajo"),

            TotalModerado = resultados.Count(x =>
                x.NivelRiesgo == "Moderado"),

            TotalAlto = resultados.Count(x =>
                x.NivelRiesgo == "Alto"),

            PuntajeGeneralPromedio = Math.Round(
                resultados.Average(x =>
                    x.PuntajeGeneral),
                2),

            Dimensiones = dimensiones
        };
    }

    private static string ConstruirCorreoHtml(
        ReporteConsolidadoRespuesta reporte)
    {
        var html = new StringBuilder();

        html.Append("""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <title>Reporte MONIPREL</title>
            </head>
            <body style="font-family: Arial, sans-serif; color: #333;">
            """);

        html.Append("""
            <h2 style="margin-bottom: 5px;">
                MONIPREL
            </h2>
            """);

        html.Append("""
            <h3 style="margin-top: 0;">
                Reporte consolidado de monitoreo preventivo
            </h3>
            """);

        html.AppendFormat(
            "<p><strong>Período:</strong> {0}</p>",
            WebUtility.HtmlEncode(reporte.Periodo));

        html.AppendFormat(
            "<p><strong>Fecha de inicio:</strong> {0:dd/MM/yyyy}</p>",
            reporte.FechaInicio);

        html.AppendFormat(
            "<p><strong>Fecha de finalización:</strong> {0:dd/MM/yyyy}</p>",
            reporte.FechaFin);

        html.Append("""
            <hr>
            <h3>Resumen general</h3>
            """);

        html.AppendFormat(
            "<p><strong>Total de evaluaciones:</strong> {0}</p>",
            reporte.TotalEvaluaciones);

        html.AppendFormat(
            "<p><strong>Nivel bajo:</strong> {0}</p>",
            reporte.TotalBajo);

        html.AppendFormat(
            "<p><strong>Nivel moderado:</strong> {0}</p>",
            reporte.TotalModerado);

        html.AppendFormat(
            "<p><strong>Nivel alto:</strong> {0}</p>",
            reporte.TotalAlto);

        html.AppendFormat(
            "<p><strong>Puntaje general promedio:</strong> {0:F2}</p>",
            reporte.PuntajeGeneralPromedio);

        if (reporte.Dimensiones.Count > 0)
        {
            html.Append("""
                <h3>Resultados por dimensión</h3>

                <table
                    style="
                        border-collapse: collapse;
                        width: 100%;
                        max-width: 700px;
                    ">

                    <thead>
                        <tr>
                            <th
                                style="
                                    border: 1px solid #ccc;
                                    padding: 8px;
                                    text-align: left;
                                ">
                                Dimensión
                            </th>

                            <th
                                style="
                                    border: 1px solid #ccc;
                                    padding: 8px;
                                    text-align: center;
                                ">
                                Puntaje promedio
                            </th>
                        </tr>
                    </thead>

                    <tbody>
                """);

            foreach (var dimension in reporte.Dimensiones)
            {
                html.AppendFormat(
                    """
                    <tr>
                        <td
                            style="
                                border: 1px solid #ccc;
                                padding: 8px;
                            ">
                            {0}
                        </td>

                        <td
                            style="
                                border: 1px solid #ccc;
                                padding: 8px;
                                text-align: center;
                            ">
                            {1:F2}
                        </td>
                    </tr>
                    """,
                    WebUtility.HtmlEncode(dimension.Dimension),
                    dimension.PuntajePromedio);
            }

            html.Append("""
                    </tbody>
                </table>
                """);
        }

        html.Append("""
            <br>

            <p style="font-size: 12px; color: #666;">
                Este reporte presenta información consolidada y agregada
                para fines de monitoreo preventivo de bienestar laboral.
                No constituye un diagnóstico clínico o psicológico y no
                contiene resultados individuales de los colaboradores.
            </p>

            <p style="font-size: 12px; color: #666;">
                Reporte generado por MONIPREL.
            </p>

            </body>
            </html>
            """);

        return html.ToString();
    }
}