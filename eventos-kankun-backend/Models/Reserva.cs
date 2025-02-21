using System;
using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID del evento es obligatorio.")]
        public int EventoId { get; set; }

        public int? SalaId { get; set; }

        public int? RecursoId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio son obligatorias.")]
        public DateTime FechaHoraInicio { get; set; }

        [Required(ErrorMessage = "La fecha y hora de fin son obligatorias.")]
        public DateTime FechaHoraFin { get; set; }

        // Relaciones
        public Evento Evento { get; set; }
        public Sala Sala { get; set; }
        public Recurso Recurso { get; set; }
    }
}