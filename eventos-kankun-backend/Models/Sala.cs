using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Sala
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la sala es obligatorio.")]
        public string Nombre { get; set; }
    }
}