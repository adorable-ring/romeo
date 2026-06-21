using IncidentReviewer.Models;
using Microsoft.EntityFrameworkCore;

namespace IncidentReviewer.Data
{
    public class IncidentDbContext(DbContextOptions<IncidentDbContext> options) : DbContext(options)
    {
        public DbSet<Incident> Incidents => Set<Incident>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Incident>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.Title).IsRequired();
                e.Property(i => i.Category).IsRequired();
                e.Property(i => i.Status).HasConversion<string>();
            });
        }
    }
}
