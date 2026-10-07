using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;
using Moniprel.Api.DTOs;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/supervisor")]
[Authorize(Roles = "Supervisor")]
public class SupervisorController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public SupervisorController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet("colaboradores")]
    public async Task<ActionResult<IEnumerable<ColaboradorAsignadoRespuesta>>>
        ObtenerColaboradores()
    {
        var identificadorUsuario =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(identificadorUsuario, out var supervisorId))
        {
            return Unauthorized(new
            {
                mensaje = "No fue posible identificar al usuario autenticado."
            });
        }

        var supervisor = await _contexto.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                x.Id == supervisorId &&
                x.Activo);

        if (supervisor is null)
        {
            return Unauthorized(new
            {
                mensaje = "El usuario autenticado no se encuentra activo."
            });
        }

        var colaboradores =
            await _contexto.AsignacionesOrganizacionales
                .AsNoTracking()
                .Where(x =>
                    x.SupervisorId == supervisorId &&
                    x.Activa &&
                    x.Colaborador.Activo)
                .OrderBy(x => x.Colaborador.Nombres)
                .ThenBy(x => x.Colaborador.Apellidos)
                .Select(x => new ColaboradorAsignadoRespuesta
                {
                    Id = x.Colaborador.Id,
                    NombreCompleto =
                        x.Colaborador.Nombres + " " +
                        x.Colaborador.Apellidos
                })
                .ToListAsync();

        return Ok(colaboradores);
    }
}