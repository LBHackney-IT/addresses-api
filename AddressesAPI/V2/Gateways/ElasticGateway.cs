using System.Collections.Generic;
using System.Linq;
using AddressesAPI.Infrastructure;
using AddressesAPI.V2.Domain;
using Amazon.Lambda.Core;
using Nest;

namespace AddressesAPI.V2.Gateways
{
    public class ElasticGateway : ISearchAddressesGateway
    {
        private readonly IElasticClient _esClient;
        private Indices.ManyIndices _indices;

        public ElasticGateway(IElasticClient esClient)
        {
            _esClient = esClient;
            _indices = Indices.Index(new List<IndexName>{"hackney_addresses", "national_addresses"});
        }

        public (List<string>, long) SearchAddresses(SearchParameters request)
        {
            var pageOffset = request.PageSize * (request.Page == 0 ? 0 : request.Page - 1);

            QueryContainer Query(QueryContainerDescriptor<QueryableAddress> q) => request.IncludeParentShells
                ? QueryIncludingParentShells(request, q)
                : BaseQuery(request, q);

            LambdaLogger.Log("Searching Elasticsearch");
            var searchResponse = await _esClient.SearchAsync<QueryableAddress>(s => s.Index(_indices)
                .Query(Query)
                .Sort(SortResults)
                .Size(request.PageSize)
                .Skip(pageOffset)
                .TrackTotalHits()).ConfigureAwait(true);
            LambdaLogger.Log(searchResponse.ApiCall.DebugInformation);
            LambdaLogger.Log($"Received {searchResponse.Documents.Count} documents");
            var addressKeys = searchResponse.Documents
                .Select(a => a.AddressKey).ToList();
            var totalCount = searchResponse.HitsMetadata.Total.Value;

            return (addressKeys, totalCount);
        }

        private QueryContainer QueryIncludingParentShells(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            var allAddressKeys = new List<string>();
            var children = _esClient.Search<QueryableAddress>(s =>
                s.Index(_indices).Query(q => BaseQuery(request, q))).Documents;

            while (true)
            {
                allAddressKeys.AddRange(children.Select(c => c.AddressKey));
                var parentUprns = children
                    .Where(d => d.ParentUPRN != null)
                    .Select(d => (long) d.ParentUPRN)
                    .Distinct().ToList();
                if (parentUprns.Count == 0) break;
                children = _esClient.Search<QueryableAddress>(s =>
                    s.Index(_indices).Query(q => SearchForMultipleUprns(parentUprns, q)))
                    .Documents;
            }

            return q.Terms(t => t
                .Field(f => f.AddressKey)
                .Terms(allAddressKeys));
        }

        private static QueryContainer BaseQuery(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            return SearchPostcodes(request, q)
                   && SearchBuildingNumbers(request, q)
                   && SearchAddressStatuses(q, request)
                   && SearchUsageCodes(request, q)
                   && SearchUsagePrimary(request, q)
                   && SearchUprns(request, q)
                   && SearchUsrns(request, q)
                   && SearchGazetteer(request, q)
                   && FilterOutOfBoroughAddresses(request, q)
                   && SearchStreet(request, q)
                   && SearchCrossReferencedUprns(request, q)
                   && FilterParentShells(request, q);
        }

        private static SortDescriptor<QueryableAddress> SortResults(SortDescriptor<QueryableAddress> srt)
        {
            return srt
                .Ascending(f => f.Town)
                .Field(f => f.Field(n => n.Postcode).Missing("_last"))
                .Ascending(f => f.Street)
                .Field(f => f.Field(n => n.PaonStartNumber).Ascending().Missing("_last"))
                .Field(f => f.Field(n => n.BuildingNumber).Ascending().Missing("_last"))
                .Field(f => f.Field(n => n.UnitNumber).Ascending().Missing("_last"))
                .Field(f => f.Field(n => n.UnitName).Ascending().Missing("_last"));
        }

        private static QueryContainer FilterParentShells(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (!request.IncludeParentShells)
            {
                return q.Term(t => t
                    .Field(f => f.PropertyShell)
                    .Value(false));
            }

            return null;
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

        private static QueryContainer SearchStreet(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (string.IsNullOrWhiteSpace(request.Street)) return null;
            var streetSearchTerm = request.Street?.Replace(" ", "").ToLower();

            var searchStreet = q.Wildcard(m =>
                m.Field(f => f.Street).Value($"*{streetSearchTerm}*"));
            return searchStreet;
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

        private static QueryContainer SearchCrossReferencedUprns(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.CrossReferencedUprns == null || request.CrossReferencedUprns.Count == 0) return null;

            return SearchForMultipleUprns(request.CrossReferencedUprns, q);
        }

        private static QueryContainer SearchForMultipleUprns(List<long> uprns, QueryContainerDescriptor<QueryableAddress> q)
        {
            return q.Terms(t => t
                .Field(f => f.UPRN)
                .Terms(uprns));
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
