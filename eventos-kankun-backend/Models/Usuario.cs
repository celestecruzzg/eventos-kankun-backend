using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApPaterno { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        public string ApMaterno { get; set; }

        [Required(ErrorMessage = "El correo electr�nico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electr�nico no es v�lido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contrase�a es obligatoria.")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "El tipo de usuario es obligatorio.")]
        public string TipoUsuario { get; set; } // "Administrador" o "Participante"

        public bool Autenticacion2FA { get; set; } = false;

        public string CodigoVerificacion { get; set; } // Para verificaci�n por correo
        public bool Verificado { get; set; } = false; // Indica si el usuario ha verificado su correo
    }
}