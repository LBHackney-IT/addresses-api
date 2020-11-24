using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V2.Domain;

namespace AddressesAPI.V2.Gateways
{
    public interface ISearchAddressesGateway
    {
        Task<(List<string>, long)> SearchAddresses(SearchParameters request);
    }
}
