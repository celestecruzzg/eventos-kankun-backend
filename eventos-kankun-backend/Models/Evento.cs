using System;
using System.ComponentModel.DataAnnotations;

namespace eventos_kankun_backend.Models
{
    public class Evento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        [Required(ErrorMessage = "La capacidad máxima es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La capacidad máxima debe ser mayor que 0.")]
        public int CapacidadMaxima { get; set; }

        public decimal Costo { get; set; } = 0.00m;

        public string Agenda { get; set; }
    }
}