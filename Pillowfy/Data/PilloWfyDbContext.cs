using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pillowfy.Models;

namespace Pillowfy.Data
{
    public class PilloWfyDbContext : IdentityDbContext<ApplicationUser>
    {
        public PilloWfyDbContext(DbContextOptions<PilloWfyDbContext> options)
            : base(options)
        {

        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Chambre> Chambres { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public DbSet<Reservation> Reservations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Reservation → Chambre
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Chambre)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.ChambreId)
                .OnDelete(DeleteBehavior.NoAction);

            // Reservation → User
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // 🔥 AJOUT IMPORTANT
            modelBuilder.Entity<Chambre>()
                .HasOne(c => c.Hotel)
                .WithMany(h => h.Chambres)
                .HasForeignKey(c => c.HotelId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}