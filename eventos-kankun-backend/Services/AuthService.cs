using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using eventos_kankun_backend.Models;
using eventos_kankun_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eventos_kankun_backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration config, EmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }

        // Registrar un nuevo usuario
        public async Task<AuthResult> RegistrarUsuarioAsync(RegistroRequest request)
        {
            // Verificar si el usuario ya existe
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (usuarioExistente != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "El correo electrónico ya está registrado."
                };
            }

            // Crear el nuevo usuario
            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                ApPaterno = request.ApPaterno,
                ApMaterno = request.ApMaterno,
                Email = request.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena), // Encriptar contraseña
                TipoUsuario = request.TipoUsuario,
                CodigoVerificacion = GenerateVerificationCode(), // Generar código de verificación
                CodigoVerificacionExpira = DateTime.UtcNow.AddMinutes(10), // Expira en 10 minutos
                Verificado = false
            };

            // Guardar el usuario en la base de datos
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Enviar el código de verificación por correo
            await _emailService.EnviarCorreoAsync(usuario.Email, "Código de Verificación", 
                $"Tu código de verificación es: {usuario.CodigoVerificacion}");

            // Retornar el resultado
            return new AuthResult
            {
                Success = true,
                Message = "Usuario registrado. Verifica tu correo.",
                CodigoVerificacion = usuario.CodigoVerificacion, // Enviar el código al frontend
                RequiereVerificacion = true // Indicar que se requiere verificación
            };
        }

        // Generar un código de verificación de 6 dígitos
        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // Iniciar sesión
        public async Task<AuthResult> LoginAsync(string email, string contrasena)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Credenciales inválidas."
                };
            }

            if (!usuario.Verificado)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Cuenta no verificada."
                };
            }

            var token = GenerarTokenJWT(usuario);

            return new AuthResult
            {
                Success = true,
                Message = "Login exitoso.",
                Token = token,
                Usuario = usuario
            };
        }

        // Generar un token JWT
        private string GenerarTokenJWT(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}