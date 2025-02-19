using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventosAPI.Models
{
    public enum EstadoEvento
    {
        Programado,
        Finalizado,
        Cancelado
    }

    public class Evento
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Nombre { get; set; }

        [Required, MaxLength(500)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [Required]
        public int CapacidadMaxima { get; set; }

        [Required]
        public decimal Costo { get; set; }

        [Required, MaxLength(300)]
        public string Agenda { get; set; }

        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EstadoEvento Estado { get; set; }
    }
}