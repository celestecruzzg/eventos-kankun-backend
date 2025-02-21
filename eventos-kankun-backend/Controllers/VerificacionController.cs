using eventos_kankun_backend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace eventos_kankun_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificacionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VerificacionController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint para verificar correo
        [HttpPost("verificar-correo")]
        [SwaggerOperation(
        Summary = "Verifica el correo electrónico del usuario.",
        Description = "Este endpoint recibe el código de verificación y verifica que coincida con el código enviado por correo electrónico."
)]
        [SwaggerResponse(200, "Correo verificado correctamente.")]
        [SwaggerResponse(400, "Código de verificación incorrecto o usuario no encontrado.")]
        public async Task<IActionResult> VerificarCorreo([FromBody] VerificacionRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Codigo))
            {
                return BadRequest(new { success = false, message = "Correo y código de verificación son obligatorios." });
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (usuario == null)
            {
                return BadRequest(new { success = false, message = "Usuario no encontrado." });
            }

            if (usuario.CodigoVerificacion == request.Codigo)
            {
                usuario.Verificado = true;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Correo verificado correctamente." });
            }

            return BadRequest(new { success = false, message = "El código de verificación es incorrecto." });
        }
    }

        public class VerificacionRequest
    {
        public string Email { get; set; }
        public string Codigo { get; set; }
    }
}
