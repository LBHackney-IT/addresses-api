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
            // EF 10 compares the compiled model to EF 6-era snapshots and warns even when
            // there is no schema change. Ignore so existing migrations still apply.
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
