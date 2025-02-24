using System;
using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Participante
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El ID del evento es obligatorio.")]
        public int EventoId { get; set; }

        [Required(ErrorMessage = "El número de asistentes es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El número de asistentes debe ser mayor que 0.")]
        public int NumeroAsistentes { get; set; }

        public string RequerimientosEspeciales { get; set; }

        public string ComprobantePago { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Usuario Usuario { get; set; }
        public Evento Evento { get; set; }
    }
}