using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

        public async Task BeforeAnyElasticsearchTest(ElasticClient client)
        {
            DeleteAddressesIndex(client);
            await CreateIndex("hackney_addresses").ConfigureAwait(true);
            await CreateIndex("national_addresses").ConfigureAwait(true);
        }
        public ElasticClient SetupElasticsearchConnection()
        {
            var settings = new ConnectionSettings(new Uri(_esDomainUri))
                .DefaultIndex("hackney_addresses")
                .PrettyJson()
                .DisableDirectStreaming()
                .ThrowExceptions();
            return new ElasticClient(settings);
        }

        private async Task CreateIndex(string name)
        {
            var settingsDoc = await File.ReadAllTextAsync("./../../../../data/elasticsearch/index.json")
                .ConfigureAwait(true);
            var httpClient = new HttpClient();
            var content = new StringContent(settingsDoc);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            await httpClient.PutAsync(new Uri(_esDomainUri + "/" + name), content)
                .ConfigureAwait(true);
            content.Dispose();
            httpClient.Dispose();
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
