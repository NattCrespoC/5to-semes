using Microsoft.EntityFrameworkCore;

namespace ASP_MVC_Prueba.Models
{
    public class CineCenterContext : DbContext
    {
        public CineCenterContext(DbContextOptions<CineCenterContext> options) : 
            base(options)
        {
        }
        
        public DbSet<Butacas> Butacas { get; set; }
        public DbSet<Facturas> Facturas { get; set; }
        public DbSet<Horarios> Horarios { get; set; }
        public DbSet<Pagos> Pagos { get; set; }
        public DbSet<Peliculas> Peliculas { get; set; }
        public DbSet<Salas> Salas { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure table names to match SQL schema
            //modelBuilder.Entity<Butacas>().ToTable("Butacas");
            //modelBuilder.Entity<Facturas>().ToTable("Facturas");
            //modelBuilder.Entity<Horarios>().ToTable("Horarios");
            //modelBuilder.Entity<Pagos>().ToTable("Pagos");
            //modelBuilder.Entity<Peliculas>().ToTable("Peliculas");
            ////modelBuilder.Entity<Salas>().ToTable("Salas");
            ////modelBuilder.Entity<Tickets>().ToTable("Tickets");
            //modelBuilder.Entity<Usuarios>().ToTable("Usuarios");
            
            //// Configure relationships and constraints
            //modelBuilder.Entity<Tickets>()
            //    .HasOne(t => t.Horario)
            //    .WithMany(h => h.Tickets)
            //    .HasForeignKey(t => t.HorarioId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Tickets>()
            //    .HasOne(t => t.Usuario)
            //    .WithMany(u => u.Tickets)
            //    .HasForeignKey(t => t.UsuarioId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Horarios>()
            //    .HasOne(h => h.Sala)
            //    .WithMany(s => s.Horarios)
            //    .HasForeignKey(h => h.SalaId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Horarios>()
            //    .HasOne(h => h.Pelicula)
            //    .WithMany(p => p.Horarios)
            //    .HasForeignKey(h => h.PeliculaId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Facturas>()
            //    .HasOne(f => f.Ticket)
            //    .WithMany(t => t.Facturas)
            //    .HasForeignKey(f => f.TicketId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Pagos>()
            //    .HasOne(p => p.Ticket)
            //    .WithMany(t => t.Pagos)
            //    .HasForeignKey(p => p.TicketId)
            //    .OnDelete(DeleteBehavior.Restrict);
                
            //modelBuilder.Entity<Butacas>()
            //    .HasOne(b => b.Sala)
            //    .WithMany(s => s.Butacas)
            //    .HasForeignKey(b => b.SalaId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
