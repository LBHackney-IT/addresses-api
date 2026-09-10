using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AddressesAPI.Infrastructure
{

    public class AddressesContext : DbContext
    {
        public AddressesContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // EF Core 9+ throws on Migrate() when the snapshot (ProductVersion 6.0.36) differs
            // from the current model after the net10.0/EF 10 upgrade. Existing migrations are still applied.
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().ToView("combined_address", "dbo");
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<CrossReference> AddressCrossReferences { get; set; }
        public DbSet<NationalAddress> NationalAddresses { get; set; }
        public DbSet<HackneyAddress> HackneyAddresses { get; set; }
    }
}
