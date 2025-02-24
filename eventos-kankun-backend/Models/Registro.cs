namespace eventos_kankun_backend.Models
{
    public class RegistroParticipanteRequest
    {
        public string Nombre { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
        public string Email { get; set; }
        public string Contrasena { get; set; }
    }

    public class RegistroAdminRequest
    {
        public string Nombre { get; set; }
        public string ApPaterno { get; set; }
        public string ApMaterno { get; set; }
        public string Email { get; set; }
        public string Contrasena { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Contrasena { get; set; }
    }
}