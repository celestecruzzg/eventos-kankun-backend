using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje)
    {
        if (string.IsNullOrEmpty(destinatario) || string.IsNullOrEmpty(asunto) || string.IsNullOrEmpty(mensaje))
        {
            throw new ArgumentException("El destinatario, el asunto y el mensaje no pueden estar vacíos.");
        }

        SmtpClient smtp = null; // Declarar la variable fuera del bloque try

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Eventos Kankun", _config["Smtp:FromEmail"]));
            email.To.Add(new MailboxAddress("", destinatario));
            email.Subject = asunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = mensaje };
            email.Body = bodyBuilder.ToMessageBody();

            smtp = new SmtpClient(); // Inicializar la variable aquí

            Console.WriteLine("Conectando al servidor SMTP...");
            await smtp.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), MailKit.Security.SecureSocketOptions.SslOnConnect);

            Console.WriteLine("Autenticando...");
            await smtp.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);

            Console.WriteLine("Enviando correo...");
            await smtp.SendAsync(email);

            Console.WriteLine("Correo enviado correctamente.");
        }
        catch (SmtpCommandException ex)
        {
            Console.WriteLine($"Error SMTP al enviar el correo: {ex.Message}");
            Console.WriteLine($"Código de estado: {ex.StatusCode}");
            throw new ApplicationException("Error al enviar el correo. Por favor, verifica la configuración SMTP.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al enviar el correo: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw new ApplicationException("Ocurrió un error inesperado al enviar el correo.", ex);
        }
        finally
        {
            if (smtp != null)
            {
                await smtp.DisconnectAsync(true);
                Console.WriteLine("Desconectado del servidor SMTP.");
            }
        }
    }
}