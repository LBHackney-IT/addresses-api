using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Domain;
using Nest;

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
                s.Query(q => SearchPostcodes(request, q)
                             && SearchBuildingNumbers(request, q)
                             && SearchAddressStatuses(q, request)
                             && SearchUsageCodes(request, q)
                             && SearchUsagePrimary(request, q)
                             && SearchUprns(request, q)
                             && SearchUsrns(request, q)
                             && SearchGazetteer(request, q)
                             && FilterOutOfBoroughAddresses(request, q))
                .Size(50)
            );

            var addressKeys = searchResponse.Documents
                .Select(a => a.AddressKey).ToList();
            var totalCount = searchResponse.Documents.Count;
            return (addressKeys, totalCount);
        }

        private static QueryContainer FilterOutOfBoroughAddresses(SearchParameters request,
            QueryContainerDescriptor<QueryableAddress> q)
        {
            return request.OutOfBoroughAddress
                ? null
                : (!q.Match(m => m.Field(f => f.Gazetteer).Query("national"))
                  && q.Term(t => t.Field(f => f.OutOfBoroughAddress).Value(false)));
        }

        private static QueryContainer SearchGazetteer(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.Gazetteer == GlobalConstants.Gazetteer.Hackney)
            {
                return q.Terms(m =>
                    m.Field(f => f.Gazetteer)
                        .Terms("hackney", "local"));
            }

            return null;
        }

        private static QueryContainer SearchPostcodes(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (string.IsNullOrWhiteSpace(request.Postcode)) return null;
            var postcodeSearchTerm = request.Postcode?.Replace(" ", "").ToLower();

            var searchPostcodes = q.Wildcard(m =>
                m.Field(f => f.Postcode).Value($"{postcodeSearchTerm}*"));
            return searchPostcodes;
        }

        private static QueryContainer SearchUsrns(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.Usrn == null) return null;

            return q.Term(t => t
                .Field(f => f.USRN)
                .Value(request.Usrn));
        }

        private static QueryContainer SearchUprns(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.Uprn == null) return null;

            return q.Term(t => t
                .Field(f => f.UPRN)
                .Value(request.Uprn));
        }

        private static QueryContainer SearchUsagePrimary(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (string.IsNullOrWhiteSpace(request.UsagePrimary)) return null;

            var usageSearchTerms = request.UsagePrimary.Split(',').Select(u => u.ToLower()).ToList();

            var searchUsagePrimary = usageSearchTerms?.Select(u =>
                    q.MatchPhrase(t =>
                        t.Field(f => f.UsagePrimary).Query(u)))
                .Aggregate((agg, qu) => agg || qu);
            return searchUsagePrimary;
        }

        private static QueryContainer SearchUsageCodes(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (string.IsNullOrWhiteSpace(request.UsageCode)) return null;

            var usageCodeSearchTerms = request.UsageCode.Split(',').Select(u => u.ToLower()).ToList();

            return usageCodeSearchTerms?.Select(u =>
                q.Wildcard(m =>
                    m.Field(f => f.UsageCode).Value($"{u}*")
                )).Aggregate((agg, qu) => agg || qu);
        }

        private static QueryContainer SearchBuildingNumbers(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (string.IsNullOrWhiteSpace(request.BuildingNumber)) return null;

            return q.Wildcard(m =>
                m.Field(f => f.BuildingNumber).Value($"*{request.BuildingNumber}*"));
        }

        private static QueryContainer SearchAddressStatuses(QueryContainerDescriptor<QueryableAddress> q, SearchParameters request)
        {
            var addressStatusQuery = (request.AddressStatus?.Select(a => a.ToLower()) ?? new[] { "approved" }).ToList();
            var addressStatusSearchTerms = addressStatusQuery.Contains("approved")
                ? addressStatusQuery.Append("approved preferred")
                : addressStatusQuery;
            return q.Terms(t => t
                .Field(f => f.AddressStatus)
                .Terms(addressStatusSearchTerms));
        }
    }
}
