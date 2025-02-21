using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventosKankun.Models
{
    public class Participante
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioID { get; set; }
        public virtual Usuario Usuario { get; set; }

        [Required]
        [ForeignKey("Evento")]
        public int EventoID { get; set; }
        public virtual Evento Evento { get; set; }

        [Required]
        public int NumeroAsistentes { get; set; }

        public string RequerimientosEspeciales { get; set; }

        [Required]
        public bool ComprobantePago { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}