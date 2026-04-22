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
     

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Chambre>();
            modelBuilder.Ignore<Avis>();
            base.OnModelCreating(modelBuilder);

            // Configuration Hotel
            modelBuilder.Entity<Hotel>()
                .HasOne(h => h.Owner)
                .WithMany(u => u.Hotels)
                .HasForeignKey(h => h.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Hotel>()
                .Property(h => h.Rating)
                .HasPrecision(3, 2);

            // Index
            modelBuilder.Entity<Hotel>()
                .HasIndex(h => h.Name);

            modelBuilder.Entity<Hotel>()
                .HasIndex(h => h.City);
        }
    }
}