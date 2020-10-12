using System.Collections.Generic;
using System.Linq;
using AddressesAPI.V1.Domain;
using AddressesAPI.V1.Factories;
using AddressesAPI.V1.Infrastructure;

namespace AddressesAPI.V1.Gateways
{
    public class CrossReferencesGateway : ICrossReferencesGateway
    {
        private readonly AddressesContext _addressesContext;
        public CrossReferencesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        public List<AddressCrossReference> GetAddressCrossReference(long uprn)
        {
            return _addressesContext.AddressCrossReferences
                .Where(x => x.UPRN.Equals(uprn))
                .Select(cr => cr.ToDomain()).ToList();
        }
    }
}
