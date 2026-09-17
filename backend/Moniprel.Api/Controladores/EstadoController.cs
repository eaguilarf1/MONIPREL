using Microsoft.AspNetCore.Mvc;

namespace Moniprel.Api.Controladores;

[ApiController]
[Route("api/[controller]")]
public class EstadoController : ControllerBase
{
    [HttpGet]
    public IActionResult ObtenerEstado()
    {
        return Ok(new
        {
            mensaje = "MONIPREL API funcionando correctamente",
            estado = "Activo"
        });
    }
}