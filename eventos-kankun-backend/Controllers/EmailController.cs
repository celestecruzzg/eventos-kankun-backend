using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public EmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("enviar")]
    [Consumes("application/json")]
    public async Task<IActionResult> EnviarCorreo([FromBody] EmailRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Destinatario) ||
            string.IsNullOrWhiteSpace(request.Asunto) || string.IsNullOrWhiteSpace(request.Mensaje))
        {
            return BadRequest("Los datos de la solicitud no son válidos.");
        }

        try
        {
            await _emailService.EnviarCorreoAsync(request.Destinatario, request.Asunto, request.Mensaje);
            return Ok(new { mensaje = "Correo enviado con éxito." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error al enviar el correo.", detalle = ex.Message });
        }
    }
}

public class EmailRequest
{
    public string Destinatario { get; set; }
    public string Asunto { get; set; }
    public string Mensaje { get; set; }
}