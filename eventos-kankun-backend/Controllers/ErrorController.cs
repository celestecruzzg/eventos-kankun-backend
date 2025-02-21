using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Controllers
{
    [ApiController]
    [Route("error")]
    public class ErrorController : ControllerBase
    {
        [HttpGet("{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return statusCode switch
            {
                404 => NotFound(new { Message = "Recurso no encontrado." }),
                400 => BadRequest(new { Message = "Solicitud inválida." }),
                500 => StatusCode(500, new { Message = "Error interno del servidor." }),
                _ => StatusCode(statusCode)
            };
        }
    }
}