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

        [SetUp]
        public async Task SetupElasticsearchClient()
        {
            ElasticsearchClient = SetupElasticsearchConnection();
            DeleteAddressesIndex(ElasticsearchClient);
            await CreateAddressesIndex().ConfigureAwait(true);
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

        public static void DeleteAddressesIndex(ElasticClient client)
        {
            if (client.Indices.Exists("addresses").Exists)
            {
                client.Indices.Delete("addresses");
            }
        }
    }
}
