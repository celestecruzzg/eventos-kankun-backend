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
        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Eventos Kankun", _config["Smtp:FromEmail"]));
            email.To.Add(new MailboxAddress("", destinatario));
            email.Subject = asunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = mensaje };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            Console.WriteLine("Conectando al servidor SMTP...");
            await smtp.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), MailKit.Security.SecureSocketOptions.SslOnConnect);
            Console.WriteLine("Autenticando...");
            await smtp.AuthenticateAsync(_config["Smtp:Username"], _config["Smtp:Password"]);
            Console.WriteLine("Enviando correo...");
            await smtp.SendAsync(email);
            Console.WriteLine("Correo enviado correctamente.");
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando correo: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            throw;
        }
    }
}