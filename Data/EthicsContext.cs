using Microsoft.EntityFrameworkCore;
using HollingerBox.Models;

namespace HollingerBox.Data
{
    public class EthicsContext : DbContext
    {
        public EthicsContext(DbContextOptions<EthicsContext> options)
            : base(options)
        {
        }

        public DbSet<Inquiry> Inquiry { get; set; }
        public DbSet<Congress> Congress { get; set; }
        public DbSet<Archivist> Archivist { get; set; }
        public DbSet<Archive> Archive { get; set; }
        public DbSet<Docs> Docs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: If you want to configure relationships / foreign keys using Fluent API
            // Example:
            // modelBuilder.Entity<Archive>()
            //    .HasOne(a => a.Inquiry)
            //    .WithMany() // or .WithMany(i => i.Archives)
            //    .HasForeignKey(a => a.Subcommittee)
            //    .OnDelete(DeleteBehavior.Cascade);

            // modelBuilder.Entity<Archive>()
            //    .HasOne(a => a.CongressRef)
            //    .WithMany() // or .WithMany(c => c.Archives)
            //    .HasForeignKey(a => a.Congress)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}