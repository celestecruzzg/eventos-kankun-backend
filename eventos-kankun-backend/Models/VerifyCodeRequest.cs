namespace eventos_kankun_backend.Models
{
    public class VerifyCodeRequest
    {
        public string Email { get; set; }
        public string CodigoVerificacion { get; set; }
    }
}
