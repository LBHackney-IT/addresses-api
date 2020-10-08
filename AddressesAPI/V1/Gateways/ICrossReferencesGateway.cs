using System.Collections.Generic;
using AddressesAPI.V1.Domain;

namespace AddressesAPI.V1.Gateways
{
    public interface ICrossReferencesGateway
    {
        public List<AddressCrossReference> GetAddressCrossReference(long uprn);
    }
}
