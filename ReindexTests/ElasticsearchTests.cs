using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Nest;
using NUnit.Framework;

namespace ReindexTests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        private readonly string _esDomainUri = "http://localhost:9202";
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

        public async Task BeforeAnyElasticsearchTest(ElasticClient client)
        {
            await DeleteAllIndices(client);
            // await CreateAddressesIndex().ConfigureAwait(true);
        }
        public ElasticClient SetupElasticsearchConnection()
        {
            var settings = new ConnectionSettings(new Uri(_esDomainUri))
                .PrettyJson()
                .DisableDirectStreaming()
                .ThrowExceptions();
            return new ElasticClient(settings);
        }

        private async Task CreateAddressesIndex()
        {
            var settingsDoc = await File.ReadAllTextAsync("./../../../../data/elasticsearch/index.json")
                .ConfigureAwait(true);
            var httpClient = new HttpClient();
            var content = new StringContent(settingsDoc);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            await httpClient.PutAsync(new Uri(_esDomainUri + "/addresses"), content)
                .ConfigureAwait(true);
            content.Dispose();
            httpClient.Dispose();
        }

        private static async Task DeleteAllIndices(ElasticClient client)
        {
            var getAllIndices = await client.Indices.GetAsync(Indices.All);
            foreach (var (name, state) in getAllIndices.Indices)
            {
                var response = await client.Indices.DeleteAsync(name);
            }
        }
    }
}
