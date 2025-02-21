using eventos_kankun_backend.Models;
using eventos_kankun_backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using MimeKit;
using MailKit.Net.Smtp;

namespace eventos_kankun_backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];
        }

        // Método para generar código de verificación
        private string GenerarCodigoVerificacion()
        {
            var guid = Guid.NewGuid();
            return guid.ToString("N").Substring(0, 6).ToUpper();
        }

        // Método para enviar el correo de verificación
        private async Task EnviarCorreoVerificacionAsync(string nombre, string email, string codigoVerificacion)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Eventos Kankun", "eventoskankun@gmail.com"));
            message.To.Add(new MailboxAddress(nombre, email));
            message.Subject = "Verificación de correo - Eventos Kankun";
            message.Body = new TextPart("Código de verificación | Eventos KANKUN")
            {
                Text = $"Hola {nombre},\n\nTu código de verificación es: {codigoVerificacion}\n\nGracias por registrarte en Eventos Kankun. Por favor, utiliza este código para verificar tu correo."
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, false);
                    await client.AuthenticateAsync("eventoskankun@gmail.com", "ut-2025uwu");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando correo: {ex.Message}");
                }
            }
        }

        // Verificar si el correo ya está registrado
        private async Task<bool> ExisteCorreoAsync(string email)
        {
            var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            return usuarioExistente != null;
        }

        // Método para generar el token JWT
        private string GenerarToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Registrar Participante
        public async Task<AuthResult> RegistrarParticipanteAsync(RegistroParticipanteRequest request)
        {
            if (await ExisteCorreoAsync(request.Email))
            {
                return new AuthResult { Success = false, Message = "El correo electrónico ya está registrado." };
            }

            var codigoVerificacion = GenerarCodigoVerificacion();

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                ApPaterno = request.ApPaterno,
                ApMaterno = request.ApMaterno,
                Email = request.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
                TipoUsuario = "Participante",
                Verificado = false,
                CodigoVerificacion = codigoVerificacion
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await EnviarCorreoVerificacionAsync(usuario.Nombre, usuario.Email, codigoVerificacion);

            return new AuthResult { Success = true, Message = "Registro exitoso. Verifica tu correo." };
        }

        // Registrar Administrador
        public async Task<AuthResult> RegistrarAdminAsync(RegistroAdminRequest request)
        {
            if (await ExisteCorreoAsync(request.Email))
            {
                return new AuthResult { Success = false, Message = "El correo electrónico ya está registrado." };
            }

            var codigoVerificacion = GenerarCodigoVerificacion();

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                ApPaterno = request.ApPaterno,
                ApMaterno = request.ApMaterno,
                Email = request.Email,
                Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
                TipoUsuario = "Admin",
                Verificado = false,
                CodigoVerificacion = codigoVerificacion
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await EnviarCorreoVerificacionAsync(usuario.Nombre, usuario.Email, codigoVerificacion);

            return new AuthResult { Success = true, Message = "Registro exitoso. Verifica tu correo." };
        }

        // Login para ambos roles
        public async Task<AuthResult> LoginAsync(string email, string contrasena)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
                return new AuthResult { Success = false, Message = "Credenciales inválidas." };

            // Generar token JWT
            var token = GenerarToken(usuario);

            return new AuthResult { Success = true, Message = "Inicio de sesión exitoso.", Token = token, Usuario = usuario };
        }

        // Autenticación con Google
        public async Task<AuthResult> LoginGoogleAsync(string tokenId)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(tokenId);
                var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (usuarioExistente == null)
                {
                    var nuevoUsuario = new Usuario
                    {
                        Nombre = payload.GivenName,
                        ApPaterno = payload.FamilyName,
                        ApMaterno = "",
                        Email = payload.Email,
                        Contrasena = "",
                        TipoUsuario = "Participante",
                        Verificado = true
                    };

                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    var token = GenerarToken(nuevoUsuario); // Generar JWT para el nuevo usuario
                    return new AuthResult { Success = true, Message = "Registro y inicio de sesión exitoso.", Token = token, Usuario = nuevoUsuario };
                }

                var tokenExistente = GenerarToken(usuarioExistente); // Generar JWT para el usuario existente
                return new AuthResult { Success = true, Message = "Inicio de sesión exitoso.", Token = tokenExistente, Usuario = usuarioExistente };
            }
            catch (InvalidJwtException)
            {
                return new AuthResult { Success = false, Message = "Token de Google inválido." };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public Usuario Usuario { get; set; }
    }
}
