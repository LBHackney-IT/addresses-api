using System.Collections.Generic;
using AddressesAPI.V2.Domain;

namespace AddressesAPI.V2.Gateways
{
    public interface IAddressesGateway
    {
        Address GetSingleAddress(string addressKey);

        (List<Address>, int) SearchAddresses(SearchParameters request);

        List<Address> GetAddresses(List<string> addressKeys, GlobalConstants.Format format);

        List<long> GetMatchingCrossReferenceUprns(string code, string value);
    }
}
