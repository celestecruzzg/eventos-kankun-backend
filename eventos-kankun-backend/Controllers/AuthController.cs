using Microsoft.AspNetCore.Mvc;
using eventos_kankun_backend.Models;
using eventos_kankun_backend.Services;
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
        public async Task<IActionResult> RegisterParticipant([FromBody] RegistroRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Los datos del registro no son válidos." });

            var result = await _authService.RegistrarUsuarioAsync(request);

            if (result.Success)
                return Ok(result); // Registro exitoso
            else
                return BadRequest(result); // Error en el registro
        }

        // Registro de un administrador
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegistroRequest request)
        {
            if (request == null)
                return BadRequest(new { Success = false, Message = "Los datos del registro no son válidos." });

            var result = await _authService.RegistrarUsuarioAsync(request); // Usar el mismo método para ambos roles

            if (result.Success)
                return Ok(result); // Registro exitoso
            else
                return BadRequest(result); // Error en el registro
        }

        // Login
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
    }
}