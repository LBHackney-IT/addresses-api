using System;
using AddressesAPI.Infrastructure;
using Nest;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        private readonly string _esDomainUri = "http://localhost:9202";
        protected ElasticClient ElasticsearchClient { get; private set; }

        [SetUp]
        public void SetupElasticsearchClient()
        {
            ElasticsearchClient = SetupElasticsearchConnection();
            DeleteAddressesIndex(ElasticsearchClient);
            CreateAddressesIndex(ElasticsearchClient);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            DeleteAddressesIndex(ElasticsearchClient);
        }

        public ElasticClient SetupElasticsearchConnection()
        {
            var settings = new ConnectionSettings(new Uri(_esDomainUri))
                .DefaultIndex("addresses")
                .PrettyJson()
                .DisableDirectStreaming()
                .ThrowExceptions();
            return new ElasticClient(settings);
        }

        private static void CreateAddressesIndex(ElasticClient client)
        {
            client.Indices.Create("addresses", c => c
                .Settings(s => s
                    .Analysis(a => a
                        .CharFilters(cf => cf
                            .PatternReplace("remove_whitespace", descriptor => descriptor.Pattern(" ").Replacement("")))
                        .Analyzers(an => an
                            .Custom("whitespace_removed", ca => ca
                                .CharFilters("remove_whitespace")
                                .Tokenizer("keyword")
                                .Filters("lowercase")
                            )
                        )
                    )
                )
                .Map<QueryableAddress>(mm => mm
                    .AutoMap()
                    .Properties(p => p
                        .Text(t => t
                            .Name(n => n.Postcode)
                            .Analyzer("whitespace_removed")
                        )
                    )
                )
            );
        }

        public static void EmptyAddressesIndex(ElasticClient client)
        {
            client.DeleteByQuery<QueryableAddress>(q => q.MatchAll());
        }


        public static void DeleteAddressesIndex(ElasticClient client)
        {
            if (client.Indices.Exists("addresses").Exists)
            {
                client.Indices.Delete("addresses");
            }
        }
    }
}
