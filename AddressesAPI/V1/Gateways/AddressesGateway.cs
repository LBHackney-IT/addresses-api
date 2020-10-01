using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Infrastructure;
using AddressCrossReference = AddressesAPI.V1.Domain.AddressCrossReference;

namespace AddressesAPI.V1.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
        private readonly AddressesContext _addressesContext;
        public AddressesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        private readonly string _connectionString;
        public AddressesGateway(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Address> GetSingleAddressAsync(string addressId)
        {
            return new Address();
        }

        public async Task<(List<Address>, int)> SearchAddressesAsync(SearchParameters request)
        {
            return (new List<Address>(), 0);
        }


        public async Task<List<AddressCrossReference>> GetAddressCrossReferenceAsync(long uprn)
        {
            return new List<AddressCrossReference>();
        }

        public async Task<(List<SimpleAddress>, int)> SearchSimpleAddressesAsync(SearchParameters request)
        {
            return (new List<SimpleAddress>(), 0);
        }





    }
}
