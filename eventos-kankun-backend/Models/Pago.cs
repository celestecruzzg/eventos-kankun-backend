using System;
using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El ID del evento es obligatorio.")]
        public int EventoId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El estado del pago es obligatorio.")]
        public string EstadoPago { get; set; } = "Pendiente"; // "Pendiente", "Pagado", "Rechazado"

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        // Relaciones
        public Usuario Usuario { get; set; }
        public Evento Evento { get; set; }
    }
}