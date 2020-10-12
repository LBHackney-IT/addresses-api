using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AddressesAPI.V1.Infrastructure
{

    public class AddressesContext : DbContext
    {
        public AddressesContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>().ToView("dbo.combined_address");
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<CrossReference> AddressCrossReferences { get; set; }
        public DbSet<NationalAddress> NationalAddresses { get; set; }
        public DbSet<HackneyAddress> HackneyAddresses { get; set; }
    }
}
