using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;
using AddressCrossReference = AddressesAPI.V1.Domain.AddressCrossReference;
#pragma warning disable 1998
// CS1998 Requires await operators with async functions.
// Remove pragma disable after setting up gateway methods to full functionality

namespace AddressesAPI.V1.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
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
