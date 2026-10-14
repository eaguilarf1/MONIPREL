using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.Interfaces;

namespace Moniprel.Api.Servicios;

public class ServicioEnvioAutomaticoReportes : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ServicioEnvioAutomaticoReportes> _logger;

    public ServicioEnvioAutomaticoReportes(
        IServiceScopeFactory scopeFactory,
        ILogger<ServicioEnvioAutomaticoReportes> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarEnviosAutomaticos(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocurrió un error durante el procesamiento automático de reportes.");
            }

            await Task.Delay(
                TimeSpan.FromHours(1),
                stoppingToken);
        }
    }

    private async Task ProcesarEnviosAutomaticos(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var contexto =
            scope.ServiceProvider.GetRequiredService<ContextoMoniprel>();

        var servicioCorreo =
            scope.ServiceProvider.GetRequiredService<IServicioCorreo>();

        var configuracion = await contexto.ConfiguracionesReporte
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (configuracion is null ||
            !configuracion.EnvioAutomatico)
        {
            return;
        }

        if (configuracion.FrecuenciaDias <= 0)
        {
            return;
        }

        var fechaActual = DateTime.UtcNow;

        if (configuracion.UltimoEnvio.HasValue)
        {
            var proximoEnvio =
                configuracion.UltimoEnvio.Value
                    .AddDays(configuracion.FrecuenciaDias);

            if (fechaActual < proximoEnvio)
            {
                return;
            }
        }

        var periodo = await contexto.PeriodosEvaluacion
            .AsNoTracking()
            .Where(x =>
                x.Activo &&
                x.FechaInicio <= fechaActual &&
                x.FechaFin >= fechaActual)
            .OrderByDescending(x => x.FechaInicio)
            .FirstOrDefaultAsync(cancellationToken);

        if (periodo is null)
        {
            return;
        }

        var resultados = await contexto.Resultados
            .AsNoTracking()
            .Where(x =>
                x.Evaluacion.PeriodoEvaluacionId == periodo.Id &&
                x.Evaluacion.Finalizada)
            .ToListAsync(cancellationToken);

        if (resultados.Count == 0)
        {
            return;
        }

        var dimensiones = await contexto.ResultadosDimension
            .AsNoTracking()
            .Where(x =>
                x.Resultado.Evaluacion.PeriodoEvaluacionId ==
                    periodo.Id &&
                x.Resultado.Evaluacion.Finalizada)
            .GroupBy(x => new
            {
                x.DimensionId,
                x.Dimension.Nombre
            })
            .Select(x => new
            {
                x.Key.DimensionId,
                Dimension = x.Key.Nombre,
                PuntajePromedio = Math.Round(
                    x.Average(y => y.Puntaje),
                    2)
            })
            .OrderBy(x => x.DimensionId)
            .ToListAsync(cancellationToken);

        var puntajeGeneral = Math.Round(
            resultados.Average(x => x.PuntajeGeneral),
            2);

        var cuerpoHtml = ConstruirCorreoHtml(
            periodo.Nombre,
            periodo.FechaInicio,
            periodo.FechaFin,
            resultados.Count,
            resultados.Count(x => x.NivelRiesgo == "Bajo"),
            resultados.Count(x => x.NivelRiesgo == "Moderado"),
            resultados.Count(x => x.NivelRiesgo == "Alto"),
            puntajeGeneral,
            dimensiones.Select(x => new DimensionCorreo
            {
                Nombre = x.Dimension,
                Puntaje = x.PuntajePromedio
            }).ToList());

        await servicioCorreo.EnviarAsync(
            configuracion.CorreoDestinatario,
            $"MONIPREL - Reporte consolidado - {periodo.Nombre}",
            cuerpoHtml);

        configuracion.UltimoEnvio = fechaActual;

        await contexto.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Reporte automático enviado correctamente para el período {PeriodoId}.",
            periodo.Id);
    }

    private static string ConstruirCorreoHtml(
        string periodo,
        DateTime fechaInicio,
        DateTime fechaFin,
        int total,
        int bajo,
        int moderado,
        int alto,
        decimal puntajeGeneral,
        List<DimensionCorreo> dimensiones)
    {
        var filasDimensiones = string.Join(
            "",
            dimensiones.Select(x =>
                $"""
                <tr>
                    <td style="border:1px solid #ccc;padding:8px;">
                        {System.Net.WebUtility.HtmlEncode(x.Nombre)}
                    </td>
                    <td style="border:1px solid #ccc;padding:8px;text-align:center;">
                        {x.Puntaje:F2}
                    </td>
                </tr>
                """));

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Arial,sans-serif;color:#333;">

                <h2>MONIPREL</h2>
                <h3>Reporte consolidado automático</h3>

                <p>
                    <strong>Período:</strong>
                    {System.Net.WebUtility.HtmlEncode(periodo)}
                </p>

                <p>
                    <strong>Fecha de inicio:</strong>
                    {fechaInicio:dd/MM/yyyy}
                </p>

                <p>
                    <strong>Fecha de finalización:</strong>
                    {fechaFin:dd/MM/yyyy}
                </p>

                <hr>

                <h3>Resumen general</h3>

                <p><strong>Total de evaluaciones:</strong> {total}</p>
                <p><strong>Nivel bajo:</strong> {bajo}</p>
                <p><strong>Nivel moderado:</strong> {moderado}</p>
                <p><strong>Nivel alto:</strong> {alto}</p>
                <p>
                    <strong>Puntaje general promedio:</strong>
                    {puntajeGeneral:F2}
                </p>

                <h3>Resultados por dimensión</h3>

                <table style="border-collapse:collapse;width:100%;max-width:700px;">
                    <thead>
                        <tr>
                            <th style="border:1px solid #ccc;padding:8px;text-align:left;">
                                Dimensión
                            </th>
                            <th style="border:1px solid #ccc;padding:8px;text-align:center;">
                                Puntaje promedio
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        {filasDimensiones}
                    </tbody>
                </table>

                <br>

                <p style="font-size:12px;color:#666;">
                    Este reporte contiene únicamente información agregada.
                    No constituye un diagnóstico clínico o psicológico.
                </p>

            </body>
            </html>
            """;
    }

    private class DimensionCorreo
    {
        public string Nombre { get; set; } = string.Empty;

        public decimal Puntaje { get; set; }
    }
}