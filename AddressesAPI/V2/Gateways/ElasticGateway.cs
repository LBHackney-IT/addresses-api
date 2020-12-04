using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            _indices = Indices.Index(new List<IndexName> { "hackney_addresses", "national_addresses" });
        }

        public async Task<(List<string>, long)> SearchAddresses(SearchParameters request)
        {
            var pageOffset = request.PageSize * (request.Page == 0 ? 0 : request.Page - 1);

            LambdaLogger.Log("Searching Elasticsearch");
            var searchResponse = await _esClient.SearchAsync<QueryableAddress>(s => s.Index(_indices)
                .Query(q => BaseQuery(request, q) || ParentShellsOfQuery(request, q))
                .Sort(SortResults)
                .Size(request.PageSize)
                .Skip(pageOffset)
                .TrackTotalHits() // This instructs elasticsearch to return the total number of documents that matched before paging was applied.
                .Explain()
            ).ConfigureAwait(true);
            LambdaLogger.Log(searchResponse.ApiCall.DebugInformation);
            LambdaLogger.Log($"Received {searchResponse.Documents.Count} documents");
            var addressKeys = searchResponse.Documents
                .Select(a => a.AddressKey).ToList();
            var totalCount = searchResponse.HitsMetadata.Total.Value;

            return (addressKeys, totalCount);
        }

        /// <summary>
        /// If the parent_shell query is set to true then we you can use this query to compile a list of UPRN's for all addresses
        /// that are parent shells of addresses in the base query, and their parent shells in turn. It will return a query
        /// that can be used to retrieve these addresses.
        /// </summary>
        /// <param name="request">parameters to use for searching</param>
        /// <param name="q">The lambda function, used by NEST, to build the query on</param>
        /// <returns></returns>
        private QueryContainer ParentShellsOfQuery(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (!request.IncludeParentShells) return null;

            var children = _esClient.Search<QueryableAddress>(s =>
                    s.Index(_indices)
                        .Query(q1 => BaseQuery(request, q1) && HasAParentShell(q1))
                        .Size(10000)
                        .Source(sf => sf.Includes(fd =>
                            fd.Fields(f => f.ParentUPRN))))
                .Documents;
            var parentUprns = new List<long>();

            LambdaLogger.Log($"Base query found {children.Count} children with parent shells");

            while (children.Count > 0)
            {
                var uprns = children
                    .Select(d => (long) d.ParentUPRN)
                    .Distinct().ToList();
                parentUprns.AddRange(uprns);
                LambdaLogger.Log($"Found {parentUprns.Count} distinct parents");

                children = _esClient.Search<QueryableAddress>(s =>
                    s.Index(_indices)
                     .Query(q => SearchForMultipleUprns(uprns, q) && HasAParentShell(q))
                     .Size(10000)
                     .Source(sf => sf.Includes(fd =>
                         fd.Fields(f => f.ParentUPRN))))
                        .Documents;
            }

            LambdaLogger.Log($"Total {parentUprns.Count} parents");

            return q.Terms(t => t
                .Field(f => f.UPRN)
                .Terms(parentUprns));
        }

        /// <summary>
        /// This method will compile a query that can be executed against Elasticsearch using all the search parameters
        /// provided, but not including any sorting or paging.
        /// </summary>
        /// <param name="request">parameters to use for searching</param>
        /// <param name="q">The lambda function, used by NEST, to build the query on</param>
        /// <returns></returns>
        private static QueryContainer BaseQuery(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            //These method all return a QueryContainer, which is a type that stores all querying methods/types that can then
            //be used to search in Elasctisearch.
            //For more information about the querying methods used in these methods, please check out elasticsearch's docs
            //https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/query-dsl.html
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
                   && FilterParentShells(request, q)
                   && FilterByModifiedSinceDate(request, q)
                   && SearchAddressFullText(request, q);
        }

        private static QueryContainer SearchAddressFullText(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.AddressQuery == null) return null;

            // This query will return addresses which have all of the search terms somewhere in the addresses, with a degree
            // of fuzziness and using allowed synonyms.
            var fuzzyMatchText = q.Match(c => c
                .Field(f => f.FullAddress)
                .Query(request.AddressQuery)
                .Analyzer("address_text")
                .Fuzziness(Fuzziness.Auto)
                .Boost(0)
                .Operator(Operator.And)
            );
            // This will ensure that numbers are matched exactly.
            var exactlyMatchNumbers = q.Match(m => m
                .Field("full_address.extracted_numbers")
                .ZeroTermsQuery(ZeroTermsQuery.All)
                .Analyzer("extract_number_analyzer")
                .Query(request.AddressQuery)
            );
            // This will boost the score of addresses which have exact matches of all search terms (i.e. no synonyms or fuzziness).
            var exactMatch = q.Match(m => m
                .Field("full_address.exact_text")
                .Analyzer("standard")
                .Query(request.AddressQuery)
                .Operator(Operator.And));
            // This will boost the score for addresses which have the exact search string, in the correct order, in one of the address lines.
            var orderedMatch = q.MatchPhrase(m => m
                .Field("full_address.exact_text")
                .Analyzer("standard")
                .Query(request.AddressQuery));
            // This will boost the score for addresses which for which one of the address lines appears in full and in the correct order
            // in the search string.
            var matchFullLines = q.MultiMatch(m => m
                .Fields(c => c
                    .Field(f => f.Line1)
                    .Field(f => f.Line2)
                    .Field(f => f.Line3)
                    .Field(f => f.Line4)
                )
                .Analyzer("standard")
                .TieBreaker(0.0)
                .ZeroTermsQuery(ZeroTermsQuery.All)
                .Query(request.AddressQuery));

            return (fuzzyMatchText && exactlyMatchNumbers && matchFullLines) || (orderedMatch) || (exactMatch);
        }

        private static SortDescriptor<QueryableAddress> SortResults(SortDescriptor<QueryableAddress> srt)
        {
            return srt
                .Descending(SortSpecialField.Score)
                .Ascending(f => f.Town)
                .Ascending(f => f.Street)
                .Field(f => f.Field(n => n.PaonStartNumber).Ascending().Missing("_last"))
                .Field(f => f.Field(n => n.UnitNumber).Ascending().Missing("_last"))
                .Field(f => f.Field(n => n.UnitName).Ascending().Missing("_last"));
        }

        private static QueryContainer FilterParentShells(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (!request.IncludeParentShells)
            {
                return q.Terms(t => t
                    .Field(f => f.PropertyShell)
                    .Terms(false)
                    );
            }

            return null;
        }

        private static QueryContainer FilterByModifiedSinceDate(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.ModifiedSince == null) return null;

            var date = Convert.ToInt32(request.ModifiedSince.Value.ToString("yyyyMMdd"));
            return q.Range(r => r.Field(f => f.PropertyChangeDate)
                       .GreaterThanOrEquals(date))
                   || q.Range(r => r.Field(f => f.PropertyStartDate)
                       .GreaterThanOrEquals(date));
        }

        private static QueryContainer HasAParentShell(QueryContainerDescriptor<QueryableAddress> q)
        {
            return q.Exists(f => f.Field(fld => fld.ParentUPRN))
                   && !q.Term(m => m.Field(f => f.ParentUPRN).Value(0));
        }

        private static QueryContainer FilterOutOfBoroughAddresses(SearchParameters request,
            QueryContainerDescriptor<QueryableAddress> q)
        {
            return request.OutOfBoroughAddress
                ? null
                : (!q.Terms(m => m.Field(f => f.Gazetteer).Terms("national"))
                  && q.Terms(t => t.Field(f => f.OutOfBoroughAddress).Terms(false)));
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

            return q.Terms(t => t
                .Field(f => f.USRN)
                .Terms(request.Usrn));
        }

        private static QueryContainer SearchUprns(SearchParameters request, QueryContainerDescriptor<QueryableAddress> q)
        {
            if (request.Uprn == null) return null;

            return q.Terms(t => t
                .Field(f => f.UPRN)
                .Terms(request.Uprn));
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
