using System.Collections.Generic;
using System.Threading.Tasks;
using AddressesAPI.V1.Domain;

namespace AddressesAPI.V1.Gateways
{
    public interface IAddressesGateway
    {
        Address GetSingleAddress(string addressKey);

        (List<Address>, int) SearchAddresses(SearchParameters request);

        (List<SimpleAddress>, int) SearchSimpleAddresses(SearchParameters request);
    }
}
