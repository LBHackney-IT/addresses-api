using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Factories;
using Address = AddressesAPI.Infrastructure.Address;

namespace AddressesAPI.V2.Gateways
{
    public class AddressesGateway : IAddressesGateway
    {
        private readonly AddressesContext _addressesContext;
        public AddressesGateway(AddressesContext addressesContext)
        {
            _addressesContext = addressesContext;
        }

        public V2.Domain.Address GetSingleAddress(string addressKey)
        {
            var addressRecord = _addressesContext.Addresses.FirstOrDefault(add => add.AddressKey.Equals(addressKey));
            return addressRecord?.ToDomain();
        }

        public List<Domain.Address> GetAddresses(List<string> addressKeys, GlobalConstants.Format format)
        {
            var addresses = _addressesContext.Addresses
                .Where(a => addressKeys.Contains(a.AddressKey)).ToList();

            return addressKeys
                .Select(a => addresses.FirstOrDefault(ad => ad.AddressKey == a))
                .Where(a => a != null)
                .Select(a => format == GlobalConstants.Format.Simple ? a.ToSimpleDomain() : a.ToDomain())
                .ToList();
        }

        public List<long> GetMatchingCrossReferenceUprns(string code, string value)
        {
            return _addressesContext.AddressCrossReferences
                .Where(cr => cr.Code == code)
                .Where(cr => cr.Value == value)
                .Select(cr => cr.UPRN)
                .ToList();
        }
    }
}
