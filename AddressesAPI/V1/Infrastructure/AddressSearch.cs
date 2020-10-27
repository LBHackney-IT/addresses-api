using Nest;

namespace AddressesAPI.V1.Infrastructure
{
    public class AddressSearch
    {
        public string Postcode { get; set; }
        public string BuildingNumber { get; set; }

    }
}
