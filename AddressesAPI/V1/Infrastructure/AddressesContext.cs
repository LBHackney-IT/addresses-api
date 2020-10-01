using Microsoft.EntityFrameworkCore;

namespace AddressesAPI.V1.Infrastructure
{

    public class AddressesContext : DbContext
    {
        public AddressesContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<GetAddressCrossReference> AddressCrossReferences { get; set; }
        public DbSet<SearchHackneyAddress> SearchHackneyAddresses { get; set; }
        public DbSet<SearchNationalAddress> SearchNationalAddresses { get; set; }

    }
}
