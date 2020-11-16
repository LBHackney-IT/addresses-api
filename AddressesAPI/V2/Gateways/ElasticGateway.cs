using System;
using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Domain;
using AddressesAPI.V2.Factories;
using Nest;
using Address = AddressesAPI.V2.Domain.Address;

namespace AddressesAPI.V2.Gateways
{
    public class ElasticGateway : IAddressesGateway
    {
        private IElasticClient _esClient;
        private AddressesContext _addressContext;

        public ElasticGateway(IElasticClient esClient, AddressesContext addressesContext)
        {
            _esClient = esClient;
            _addressContext = addressesContext;
        }

        public (List<Address>, int) SearchAddresses(SearchParameters request)
        {
            var documents = _esClient
                .Search<QueryableAddress>(s => s.Size(2000)).Documents;
            var addressKeys = documents
                .Select(a => a.AddressKey);
            var addresses = _addressContext.Addresses
                .Where(a => addressKeys.Contains(a.AddressKey))
                .Select(a => a.ToDomain())
                .ToList();
            return (addresses, documents.Count);
        }

        Address IAddressesGateway.GetSingleAddress(string addressKey)
        {
            return new Address();
        }
    }
}
