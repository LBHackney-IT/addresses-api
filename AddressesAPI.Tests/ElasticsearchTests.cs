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
            // #region agent log
            AgentLog("E", "ElasticsearchTests.cs:RunAfterAnyTests", "teardown", new
            {
                result = TestContext.CurrentContext?.Result?.Outcome?.Status.ToString(),
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
            DeleteAddressesIndex(ElasticsearchClient);
        }

        public static async Task BeforeAnyElasticsearchTest(ElasticClient client)
        {
            // #region agent log
            AgentLog("B", "ElasticsearchTests.cs:BeforeAnyElasticsearchTest", "setup-enter", new
            {
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
            DeleteAddressesIndex(client);
            await CreateIndex("hackney_addresses", client).ConfigureAwait(true);
            await CreateIndex("national_addresses", client).ConfigureAwait(true);
        }
        public static ElasticClient SetupElasticsearchConnection()
        {
            var esDomainUri = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL")
                              ?? "http://localhost:9202";
            var pool = new SingleNodeConnectionPool(new Uri(esDomainUri));
            var settings = new ConnectionSettings(pool).PrettyJson()
                .DisableDirectStreaming()
                .SniffOnStartup(false)
                .ThrowExceptions();
            var client = new ElasticClient(settings);
            // #region agent log
            AgentLog("C", "ElasticsearchTests.cs:SetupElasticsearchConnection", "client-created", new
            {
                esDomainUri,
                pid = Environment.ProcessId
            });
            // #endregion
            return client;
        }

        private static async Task CreateIndex(string name, IElasticClient client)
        {
            var settingsDoc = await File.ReadAllTextAsync("./../../../../data/elasticsearch/index.json")
                .ConfigureAwait(true);

            // #region agent log
            var existsBefore = client.Indices.Exists(name);
            AgentLog("C", "ElasticsearchTests.cs:CreateIndex", "create-before", new
            {
                index = name,
                exists = existsBefore.Exists,
                isValid = existsBefore.IsValid,
                status = existsBefore.ApiCall?.HttpStatusCode,
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
            try
            {
                await client.LowLevel.Indices.CreateAsync<BytesResponse>(name, settingsDoc)
                    .ConfigureAwait(true);
            }
            catch (Exception ex) when (IsResourceAlreadyExists(ex))
            {
                // #region agent log
                AgentLog("D", "ElasticsearchTests.cs:CreateIndex", "create-already-exists-ignored", new
                {
                    index = name,
                    error = ex.GetType().Name,
                    msg = ex.Message,
                    existsBefore = existsBefore.Exists,
                    test = TestContext.CurrentContext?.Test?.FullName,
                    thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                    pid = Environment.ProcessId
                });
                // #endregion
            }
            catch (Exception ex)
            {
                // #region agent log
                AgentLog("C", "ElasticsearchTests.cs:CreateIndex", "create-fail", new
                {
                    index = name,
                    error = ex.GetType().Name,
                    msg = ex.Message,
                    existsBefore = existsBefore.Exists,
                    test = TestContext.CurrentContext?.Test?.FullName,
                    thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                    pid = Environment.ProcessId
                });
                // #endregion
                throw;
            }

            var health = client.Cluster.Health(name, h => h
                .WaitForStatus(WaitForStatus.Yellow)
                .Timeout("10s"));
            // #region agent log
            AgentLog("D", "ElasticsearchTests.cs:CreateIndex", "health-after-create", new
            {
                index = name,
                status = health.Status.ToString(),
                isValid = health.IsValid,
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
        }

        public static void DeleteAddressesIndex(ElasticClient client)
        {
            DeleteOneIndex(client, "hackney_addresses");
            DeleteOneIndex(client, "national_addresses");
        }

        private static void DeleteOneIndex(ElasticClient client, string name)
        {
            var exists = client.Indices.Exists(name);
            // #region agent log
            AgentLog("C", "ElasticsearchTests.cs:DeleteOneIndex", "exists-check", new
            {
                index = name,
                exists = exists.Exists,
                isValid = exists.IsValid,
                status = exists.ApiCall?.HttpStatusCode,
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
            var del = client.Indices.Delete(name, d => d.IgnoreUnavailable());
            // #region agent log
            AgentLog("D", "ElasticsearchTests.cs:DeleteOneIndex", "delete-result", new
            {
                index = name,
                acknowledged = del.Acknowledged,
                isValid = del.IsValid,
                status = del.ApiCall?.HttpStatusCode,
                test = TestContext.CurrentContext?.Test?.FullName,
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId,
                pid = Environment.ProcessId
            });
            // #endregion
        }

        private static bool IsResourceAlreadyExists(Exception ex)
        {
            return ex.Message.IndexOf("resource_already_exists_exception", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        // #region agent log
        internal static void AgentLog(string hypothesisId, string location, string message, object data)
        {
            try
            {
                var line = System.Text.Json.JsonSerializer.Serialize(new
                {
                    sessionId = "9da333",
                    runId = "verify-all",
                    hypothesisId,
                    location,
                    message,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    data
                });
                var logPaths = new[]
                {
                    "/Users/thatgui/Documents/SoftwareEngineer/hackney/addresses-api/.cursor/debug-9da333.log",
                    "/app/.cursor/debug-9da333.log"
                };
                foreach (var logPath in logPaths)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                        File.AppendAllText(logPath, line + "\n");
                    }
                    catch
                    {
                        // try the next path
                    }
                }
                try
                {
                    using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
                    foreach (var url in new[] { "http://127.0.0.1:7892/ingest/9522fbfe-1b26-496d-ac77-f9a27da282a4", "http://host.docker.internal:7892/ingest/9522fbfe-1b26-496d-ac77-f9a27da282a4" })
                    {
                        try
                        {
                            using var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, url);
                            req.Headers.Add("X-Debug-Session-Id", "9da333");
                            req.Content = new System.Net.Http.StringContent(line, System.Text.Encoding.UTF8, "application/json");
                            client.Send(req);
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
        }
        // #endregion
    }
}
