using ASN.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ASN.Infrastructure.Data
{
    public class AsnContext(DbContextOptions<AsnContext> options) : DbContext(options)
    {
        public DbSet<BoxHeader> Countries { get; set; }
        public DbSet<BoxLine> Matchups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BoxHeader>()
                .HasMany(e => e.BoxLines)
                .WithOne(e => e.header)
                .HasForeignKey(e => e.BoxHeaderId);
        }
    }
}
