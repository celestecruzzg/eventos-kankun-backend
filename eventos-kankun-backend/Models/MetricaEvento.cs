using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class MetricaEvento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID del evento es obligatorio.")]
        public int EventoId { get; set; }

        public int NumeroParticipantes { get; set; } = 0;

        public decimal IngresosGenerados { get; set; } = 0.00m;

        public int Cancelaciones { get; set; } = 0;

        public int Ausencias { get; set; } = 0;

        // Relación
        public Evento Evento { get; set; }
    }
}