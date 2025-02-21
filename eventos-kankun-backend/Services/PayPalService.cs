using PayPal.Api;

namespace eventos_kankun_backend.Services
{
    public class PayPalService
    {
        private readonly APIContext _apiContext;

        public PayPalService(IConfiguration configuration)
        {
            var config = new Dictionary<string, string>
            {
                { "mode", configuration["PayPal:Mode"] }
            };

            var clientId = configuration["PayPal:ClientId"];
            var clientSecret = configuration["PayPal:ClientSecret"];

            var accessToken = new OAuthTokenCredential(clientId, clientSecret, config).GetAccessToken();
            _apiContext = new APIContext(accessToken) { Config = config };
        }

        public Payment CreatePayment(decimal amount, string returnUrl, string cancelUrl)
        {
            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        amount = new Amount
                        {
                            currency = "USD",
                            total = amount.ToString("F2")
                        },
                        description = "Pago para evento"
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    return_url = returnUrl,
                    cancel_url = cancelUrl
                }
            };

            return payment.Create(_apiContext);
        }

        public Payment ExecutePayment(string paymentId, string payerId)
        {
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };
            return payment.Execute(_apiContext, paymentExecution);
        }
    }
}