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
            var searchResponse = _esClient.Search<QueryableAddress>(s =>
                s.Query(q =>
                        q.Wildcard(m =>
                    {
                        var postcodeSearchTerm = request.Postcode?.Replace(" ", "").ToLower();
                        return m
                            .Field("postcode")
                            .Value($"{postcodeSearchTerm}*");
                    })
                )
                .Size(50)
            );

            var addressKeys = searchResponse.Documents
                .Select(a => a.AddressKey).ToList();
            var totalCount = searchResponse.Documents.Count;
            return (addressKeys, totalCount);
        }
    }
}
