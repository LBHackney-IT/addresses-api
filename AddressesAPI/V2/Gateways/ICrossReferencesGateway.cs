using System.Collections.Generic;
using AddressesAPI.V2.Domain;

namespace AddressesAPI.V2.Gateways
{
    public interface ICrossReferencesGateway
    {
        public List<AddressCrossReference> GetAddressCrossReference(long uprn);
    }
}
