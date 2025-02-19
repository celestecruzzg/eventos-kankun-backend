using EventosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventosAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Evento> Eventos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Evento>().ToTable("Eventos");

            // Especificamos precisi�n y escala para la propiedad 'Costo'
            modelBuilder.Entity<Evento>()
                .Property(e => e.Costo)
                .HasColumnType("decimal(10,2)"); // 10 d�gitos en total, 2 de ellos despu�s del punto decimal
        }
    }
}
