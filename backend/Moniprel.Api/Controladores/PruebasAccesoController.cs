using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/pruebas-acceso")]
public class PruebasAccesoController : ControllerBase
{
    [HttpGet("autenticado")]
    [Authorize]
    public IActionResult Autenticado()
    {
        return Ok(new
        {
            mensaje = "Usuario autenticado correctamente."
        });
    }

    [HttpGet("administrador")]
    [Authorize(Roles = "Administrador")]
    public IActionResult SoloAdministrador()
    {
        return Ok(new
        {
            mensaje = "Acceso autorizado para Administrador."
        });
    }

    [HttpGet("supervisor")]
    [Authorize(Roles = "Supervisor")]
    public IActionResult SoloSupervisor()
    {
        return Ok(new
        {
            mensaje = "Acceso autorizado para Supervisor."
        });
    }
}