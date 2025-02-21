using eventos_kankun_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace eventos_kankun_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Participante> Participantes { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<MetricaEvento> MetricasEventos { get; set; }
        public DbSet<Pago> Pagos { get; set; }
    }
}