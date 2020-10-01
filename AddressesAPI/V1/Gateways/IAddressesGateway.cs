using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;

namespace AddressesAPI.V1.Gateways
{
    public interface IAddressesGateway
    {
        Task<Address> GetSingleAddressAsync(string addressId);

        Task<(List<Address>, int)> SearchAddressesAsync(SearchParameters request);

        Task<(List<SimpleAddress>, int)> SearchSimpleAddressesAsync(SearchParameters request);

        Task<List<AddressCrossReference>> GetAddressCrossReferenceAsync(long uprn);
    }
}
