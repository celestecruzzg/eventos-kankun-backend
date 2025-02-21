using Microsoft.EntityFrameworkCore;
using EventosKankun.Models;

namespace EventosKankun.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Participante> Participantes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Participante>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participante>()
                .HasOne(p => p.Evento)
                .WithMany()
                .HasForeignKey(p => p.EventoID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}