using eventos_kankun_backend.Models;
using eventos_kankun_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using eventos_kankun_backend.Data;

namespace eventos_kankun_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context; // Contexto de la base de datos

        public AuthController(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        // Registro de un participante
        [HttpPost("register-participant")]
        [SwaggerOperation(Summary = "Registro de participante")]
        public async Task<IActionResult> RegisterParticipant([FromBody] RegistroParticipanteRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Los datos del registro no son válidos." });

            var result = await _authService.RegistrarParticipanteAsync(request);

            if (result.Success)
                return Ok(result); // Registro exitoso
            else
                return BadRequest(result); // Error en el registro
        }

        // Registro de un administrador
        [HttpPost("register-admin")]
        [SwaggerOperation(Summary = "Registro de administrador")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegistroAdminRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Los datos del registro no son válidos." });

            var result = await _authService.RegistrarAdminAsync(request); // Método específico para admins

            if (result.Success)
                return Ok(result); // Registro exitoso
            else
                return BadRequest(result); // Error en el registro
        }

        // Login para los participantes y administradores
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Login de usuario")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Las credenciales no son válidas." });

            var result = await _authService.LoginAsync(request.Email, request.Contrasena);

            if (result.Success)
                return Ok(new { result.Success, result.Message, Token = result.Token }); // Enviar token en la respuesta
            else
                return Unauthorized(result); // Error en el login
        }

        // Verificación de código para 2FA y registro
        [HttpPost("verify-code")]
        [SwaggerOperation(Summary = "Verificación de código de 2FA o registro")]
        public async Task<IActionResult> VerifyCodeAsync([FromBody] VerifyCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.CodigoVerificacion))
            {
                return BadRequest(new { Success = false, Message = "Correo electrónico y código de verificación son requeridos." });
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Verificar si el usuario existe
            if (usuario == null)
            {
                return NotFound(new { Success = false, Message = "Usuario no encontrado." });
            }

            // Verificar si el código de verificación de 2FA es correcto
            if (!string.IsNullOrEmpty(usuario.CodigoVerificacion2FA) && usuario.CodigoVerificacion2FA == request.CodigoVerificacion)
            {
                usuario.CodigoVerificacion2FA = null;
                await _context.SaveChangesAsync();

                var token = _authService.GenerarToken(usuario);
                return Ok(new { Success = true, Message = "Código 2FA verificado con éxito.", Token = token, Usuario = usuario });
            }

            // Verificar si el código de verificación de registro es correcto
            if (!string.IsNullOrEmpty(usuario.CodigoVerificacionRegistro) && usuario.CodigoVerificacionRegistro == request.CodigoVerificacion)
            {
                usuario.Verificado = true;
                usuario.CodigoVerificacionRegistro = null;
                await _context.SaveChangesAsync();

                var token = _authService.GenerarToken(usuario);
                return Ok(new { Success = true, Message = "Cuenta verificada con éxito.", Token = token, Usuario = usuario });
            }

            return BadRequest(new { Success = false, Message = "Código de verificación incorrecto." });
        }

    }
}
