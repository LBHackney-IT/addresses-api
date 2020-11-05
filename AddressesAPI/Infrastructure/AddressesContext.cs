using Microsoft.EntityFrameworkCore;

namespace AddressesAPI.Infrastructure
{

    public class AddressesContext : DbContext
    {
        public AddressesContext(DbContextOptions options) : base(options)
        {

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
