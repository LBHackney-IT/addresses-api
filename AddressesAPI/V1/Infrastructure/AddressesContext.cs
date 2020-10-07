using Microsoft.EntityFrameworkCore;

namespace AddressesAPI.V1.Infrastructure
{

    public class AddressesContext : DbContext
    {
        public AddressesContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CrossReference> AddressCrossReferences { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}
