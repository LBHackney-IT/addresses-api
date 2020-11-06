using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;

namespace AddressesAPI.V2.Gateways
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
