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
        public DbSet<Paiement> Paiements { get; set; }


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
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 AJOUT IMPORTANT
            modelBuilder.Entity<Chambre>()
                .HasOne(c => c.Hotel)
                .WithMany(h => h.Chambres)
                .HasForeignKey(c => c.HotelId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Fluent API : Paiement ──────────────────────────────────
            modelBuilder.Entity<Paiement>(e =>
            {
                e.Property(p => p.Montant)
                    .HasColumnType("decimal(10,2)");

                e.Property(p => p.Methode)
                    .HasConversion<string>();

                e.Property(p => p.Statut)
                    .HasConversion<string>();

                // 1 Reservation = 1 Paiement max
                e.HasOne(p => p.Reservation)
                    .WithOne()
                    .HasForeignKey<Paiement>(p => p.ReservationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Fluent API : Avis ──────────────────────────────────────
            modelBuilder.Entity<Avis>(e =>
            {
                // 1 user = 1 seul avis par hotel
                e.HasIndex(a => new { a.UserId, a.HotelId })
                    .IsUnique();

                e.HasOne(a => a.Hotel)
                    .WithMany()
                    .HasForeignKey(a => a.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}