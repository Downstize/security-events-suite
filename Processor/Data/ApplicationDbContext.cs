using Microsoft.EntityFrameworkCore;
using Processor.Models;

namespace Processor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Incident> Incidents { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Incident>()
                .HasMany(i => i.Events)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}