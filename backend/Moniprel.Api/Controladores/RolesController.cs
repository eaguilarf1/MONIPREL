using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Datos;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrador")]
public class RolesController : ControllerBase
{
    private readonly ContextoMoniprel _contexto;

    public RolesController(ContextoMoniprel contexto)
    {
        _contexto = contexto;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var roles = await _contexto.Roles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Nombre,
                x.Descripcion,
                x.Activo
            })
            .ToListAsync();

        return Ok(roles);
    }
}