using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using NUnit.Framework;

namespace ReindexTests
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
        public async Task RunAfterAnyTests()
        {
            await DeleteAllIndices(ElasticsearchClient);
        }

        private static async Task BeforeAnyElasticsearchTest(IElasticClient client)
        {
            await DeleteAllIndices(client);
        }

        private static ElasticClient SetupElasticsearchConnection()
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

        private static async Task DeleteAllIndices(IElasticClient client)
        {
            var getAllIndices = await client.Indices.GetAsync(Indices.All);
            foreach (var (name, state) in getAllIndices.Indices)
            {
                var response = await client.Indices.DeleteAsync(name);
            }
        }
    }
}
