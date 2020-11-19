using System.Collections.Generic;
using AddressesAPI.V2.Domain;

namespace AddressesAPI.V2.Gateways
{
    public interface ISearchAddressesGateway
    {
        (List<string>, long) SearchAddresses(SearchParameters request);
    }
}
