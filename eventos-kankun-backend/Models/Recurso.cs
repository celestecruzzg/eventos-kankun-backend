using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Recurso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
        public string Nombre { get; set; }
    }
}