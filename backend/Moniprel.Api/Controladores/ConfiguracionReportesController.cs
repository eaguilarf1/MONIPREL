using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/configuracion-reportes")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionReportesController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public ConfiguracionReportesController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracionReporteRespuesta>>
        Obtener()
    {
        var configuracion = await _contexto.ConfiguracionesReporte
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (configuracion is null)
        {
            return NotFound(new
            {
                mensaje = "Todavía no existe una configuración de reportes."
            });
        }

        return Ok(Mapear(configuracion));
    }

    [HttpPut]
    public async Task<ActionResult<ConfiguracionReporteRespuesta>>
        Actualizar(ActualizarConfiguracionReporteSolicitud solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.CorreoDestinatario))
        {
            return BadRequest(new
            {
                mensaje = "Debe indicar un correo destinatario."
            });
        }

        try
        {
            _ = new MailAddress(solicitud.CorreoDestinatario);
        }
        catch
        {
            return BadRequest(new
            {
                mensaje = "El correo destinatario no tiene un formato válido."
            });
        }

        if (solicitud.FrecuenciaDias <= 0)
        {
            return BadRequest(new
            {
                mensaje = "La frecuencia debe ser mayor que cero días."
            });
        }

        var configuracion = await _contexto.ConfiguracionesReporte
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (configuracion is null)
        {
            configuracion = new ConfiguracionReporte
            {
                CorreoDestinatario = solicitud.CorreoDestinatario.Trim(),
                EnvioAutomatico = solicitud.EnvioAutomatico,
                FrecuenciaDias = solicitud.FrecuenciaDias,
                FechaActualizacion = DateTime.UtcNow
            };

            _contexto.ConfiguracionesReporte.Add(configuracion);
        }
        else
        {
            configuracion.CorreoDestinatario =
                solicitud.CorreoDestinatario.Trim();

            configuracion.EnvioAutomatico =
                solicitud.EnvioAutomatico;

            configuracion.FrecuenciaDias =
                solicitud.FrecuenciaDias;

            configuracion.FechaActualizacion =
                DateTime.UtcNow;
        }

        await _contexto.SaveChangesAsync();

        return Ok(Mapear(configuracion));
    }

    private static ConfiguracionReporteRespuesta Mapear(
        ConfiguracionReporte configuracion)
    {
        return new ConfiguracionReporteRespuesta
        {
            Id = configuracion.Id,
            CorreoDestinatario = configuracion.CorreoDestinatario,
            EnvioAutomatico = configuracion.EnvioAutomatico,
            FrecuenciaDias = configuracion.FrecuenciaDias,
            FechaActualizacion = configuracion.FechaActualizacion,
            UltimoEnvio = configuracion.UltimoEnvio
        };
    }
}