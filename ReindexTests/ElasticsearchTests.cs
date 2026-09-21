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
            var pool = new SingleNodeConnectionPool(new Uri(esDomainUri));
            var settings = new ConnectionSettings(pool).PrettyJson()
                .DisableDirectStreaming()
                .SniffOnStartup(false)
                .ThrowExceptions();
            return new ElasticClient(settings);
        }

        private static async Task DeleteAllIndices(IElasticClient client)
        {
            var getAllIndices = await client.Indices.GetAsync(Indices.All);
            // #region agent log
            try
            {
                var names = new System.Collections.Generic.List<string>();
                foreach (var (name, state) in getAllIndices.Indices)
                {
                    names.Add(name.Name);
                }
                var line = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "9da333",
                    runId = "verify-all",
                    hypothesisId = "A",
                    location = "ReindexTests/ElasticsearchTests.cs:DeleteAllIndices",
                    message = "reindex-delete-all",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    data = new
                    {
                        indices = names,
                        test = TestContext.CurrentContext?.Test?.FullName,
                        thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                        pid = Environment.ProcessId
                    }
                });
                var logPaths = new[]
                {
                    "/Users/thatgui/Documents/SoftwareEngineer/hackney/addresses-api/.cursor/debug-9da333.log",
                    "/app/.cursor/debug-9da333.log"
                };
                foreach (var candidate in logPaths)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(candidate));
                        File.AppendAllText(candidate, line + "\n");
                    }
                    catch
                    {
                        // try the next path
                    }
                }
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
                    foreach (var url in new[] { "http://127.0.0.1:7892/ingest/9522fbfe-1b26-496d-ac77-f9a27da282a4", "http://host.docker.internal:7892/ingest/9522fbfe-1b26-496d-ac77-f9a27da282a4" })
                    {
                        try
                        {
                            using var req = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                            req.Headers.Add("X-Debug-Session-Id", "9da333");
                            req.Content = new StringContent(line, System.Text.Encoding.UTF8, "application/json");
                            http.Send(req);
                        }
                        catch
                        {
                            // ingest is best-effort
                        }
                    }
                }
                catch
                {
                    // ingest is best-effort
                }
            }
            catch
            {
                // ignore debug log failures
            }
            // #endregion
            foreach (var (name, state) in getAllIndices.Indices)
            {
                if (name.Name == "hackney_addresses" || name.Name == "national_addresses")
                {
                    continue;
                }
                var response = await client.Indices.DeleteAsync(name);
            }
        }
    }
}
