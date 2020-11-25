using System;
using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        protected ElasticClient ElasticsearchClient { get; private set; }

        [OneTimeSetUp]
        public void BeforeAllElasticsearchTests()
        {
            ElasticsearchClient = SetupElasticsearchConnection();
        }

        [SetUp]
        public async Task SetupElasticsearchClient()
        {
            await BeforeAnyElasticsearchTest(ElasticsearchClient).ConfigureAwait(true);
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            DeleteAddressesIndex(ElasticsearchClient);
        }

        public static async Task BeforeAnyElasticsearchTest(ElasticClient client)
        {
            DeleteAddressesIndex(client);
            await CreateIndex("hackney_addresses", client).ConfigureAwait(true);
            await CreateIndex("national_addresses", client).ConfigureAwait(true);
        }
        public static ElasticClient SetupElasticsearchConnection()
        {
            var esDomainUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL")
                              ?? "http://localhost:9202";
            using var pool = new SingleNodeConnectionPool(new Uri(esDomainUri));
            using var settings = new ConnectionSettings(pool).PrettyJson()
                .DisableDirectStreaming()
                .SniffOnStartup(false)
                .ThrowExceptions();
            return new ElasticClient(settings);
        }

        private static async Task CreateIndex(string name, IElasticClient client)
        {
            var settingsDoc = await File.ReadAllTextAsync("./../../../../data/elasticsearch/index.json")
                .ConfigureAwait(true);

            await client.LowLevel.Indices.CreateAsync<BytesResponse>(name, settingsDoc)
                .ConfigureAwait(true);
        }

        public static void DeleteAddressesIndex(ElasticClient client)
        {
            if (client.Indices.Exists("hackney_addresses").Exists)
            {
                client.Indices.Delete("hackney_addresses");
            }

            if (client.Indices.Exists("national_addresses").Exists)
            {
                client.Indices.Delete("national_addresses");
            }
        }
    }
}
