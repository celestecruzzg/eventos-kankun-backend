using eventos_kankun_backend.Models;

namespace eventos_kankun_backend.Services
{
    public class AuthResult
    {
        public bool Success { get; set; } // Indica si la operación fue exitosa
        public string Message { get; set; } // Mensaje descriptivo (éxito o error)
        public string Token { get; set; } // Token JWT (para el login)
        public Usuario Usuario { get; set; } // Datos del usuario (opcional)

        // Nuevos campos para la validación de cuenta
        public string CodigoVerificacion { get; set; } // Código de verificación (para el registro)
        public bool RequiereVerificacion { get; set; } // Indica si el usuario necesita verificar su cuenta
    }
}