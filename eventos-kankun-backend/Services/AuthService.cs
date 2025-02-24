using eventos_kankun_backend.Data;
using eventos_kankun_backend.Models;
using eventos_kankun_backend.Services;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _jwtKey = configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt:Audience"];
        _configuration = configuration;
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
        message.From.Add(new MailboxAddress("Eventos Kankun", _configuration["Smtp:FromEmail"]));
        message.To.Add(new MailboxAddress(nombre, email));
        message.Subject = "Verificación de correo - Eventos Kankun";
        message.Body = new TextPart("plain")
        {
            Text = $"Hola {nombre},\n\nTu código de verificación es: {codigoVerificacion}\n\nGracias por registrarte en Eventos Kankun. Por favor, utiliza este código para verificar tu correo."
        };

        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), MailKit.Security.SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                Console.WriteLine("Correo enviado correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw; // Re-lanza la excepción para que no se pierda en el flujo
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
}