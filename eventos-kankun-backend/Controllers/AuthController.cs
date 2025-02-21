using eventos_kankun_backend.Models;
using eventos_kankun_backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace eventos_kankun_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Registro de un participante
        [HttpPost("register-participant")]
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

        // Login para los participantes
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Las credenciales no son válidas." });

            var result = await _authService.LoginAsync(request.Email, request.Contrasena);

            if (result.Success)
                return Ok(result); // Login exitoso
            else
                return Unauthorized(result); // Error en el login
        }

        // Autenticación con Google
        [HttpPost("login-google")]
        public async Task<IActionResult> LoginGoogle([FromBody] GoogleLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.TokenId))
                return BadRequest(new { Success = false, Message = "El token de Google no es válido." });

            var result = await _authService.LoginGoogleAsync(request.TokenId);

            if (result.Success)
                return Ok(result); // Login exitoso
            else
                return Unauthorized(result); // Error en el login con Google
        }
    }

    // Modelos para las solicitudes

    
}
