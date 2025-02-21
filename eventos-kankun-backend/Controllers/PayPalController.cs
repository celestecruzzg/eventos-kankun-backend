using Microsoft.AspNetCore.Mvc;
using eventos_kankun_backend.Services;

namespace eventos_kankun_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalController : ControllerBase
    {
        private readonly PayPalService _payPalService;

        public PayPalController(PayPalService payPalService)
        {
            _payPalService = payPalService;
        }

        [HttpPost("crear-pago")]
        public IActionResult CrearPago(decimal amount)
        {
            var returnUrl = "http://localhost:3000/pago-exitoso";
            var cancelUrl = "http://localhost:3000/pago-cancelado";

            var payment = _payPalService.CreatePayment(amount, returnUrl, cancelUrl);
            return Ok(payment);
        }

        [HttpPost("ejecutar-pago")]
        public IActionResult EjecutarPago(string paymentId, string payerId)
        {
            var payment = _payPalService.ExecutePayment(paymentId, payerId);
            return Ok(payment);
        }
    }
}