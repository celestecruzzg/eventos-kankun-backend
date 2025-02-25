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

    private string GenerarCodigoVerificacion()
    {
        var guid = Guid.NewGuid();
        return guid.ToString("N").Substring(0, 6).ToUpper();
    }

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                throw;
            }
        }
    }

    private async Task<bool> ExisteCorreoAsync(string email)
    {
        var usuarioExistente = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        return usuarioExistente != null;
    }

    public string GenerarToken(Usuario usuario)
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
            CodigoVerificacionRegistro = codigoVerificacion,
            CodigoVerificacion2FA = null
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        await EnviarCorreoVerificacionAsync(usuario.Nombre, usuario.Email, codigoVerificacion);

        return new AuthResult { Success = true, Message = "Registro exitoso. Verifica tu correo." };
    }

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
            CodigoVerificacionRegistro = codigoVerificacion,
            CodigoVerificacion2FA = null
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        await EnviarCorreoVerificacionAsync(usuario.Nombre, usuario.Email, codigoVerificacion);

        return new AuthResult { Success = true, Message = "Registro exitoso. Verifica tu correo." };
    }

    public async Task<AuthResult> LoginAsync(string email, string contrasena)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(contrasena, usuario.Contrasena))
            return new AuthResult { Success = false, Message = "Credenciales inválidas." };

        if (!usuario.Verificado)
            return new AuthResult { Success = false, Message = "Por favor, verifica tu correo antes de iniciar sesión." };

        usuario.CodigoVerificacion2FA = GenerarCodigoVerificacion();
        await _context.SaveChangesAsync();

        await EnviarCorreoVerificacionAsync(usuario.Nombre, usuario.Email, usuario.CodigoVerificacion2FA);

        return new AuthResult { Success = true, Message = "Se ha enviado un código de verificación a tu correo. Ingrésalo para completar el inicio de sesión." };
    }
}
