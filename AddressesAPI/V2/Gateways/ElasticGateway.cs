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
    public class ElasticGateway : ISearchAddressesGateway
    {
        private IElasticClient _esClient;

        public ElasticGateway(IElasticClient esClient)
        {
            _esClient = esClient;
        }

        public (List<string>, int) SearchAddresses(SearchParameters request)
        {
            var documents = _esClient
                .Search<QueryableAddress>(s => s.Size(2000)).Documents;
            var addressKeys = documents
                .Select(a => a.AddressKey).ToList();
            var totalCount = documents.Count;
            return (addressKeys, totalCount);
        }
    }
}
