using System;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nest;
using Npgsql;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    public class IntegrationTests<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected AddressesContext DatabaseContext { get; private set; }
        protected ElasticClient ElasticsearchClient { get; private set; }
        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL") == null)
            {
                Environment.SetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL", "http://localhost:9202");
            }

            ElasticsearchClient = ElasticsearchTests.SetupElasticsearchConnection();
            ConnectToPostgresDbUsingEf();
        }

        [SetUp]
        public async Task BaseSetup()
        {
            Environment.SetEnvironmentVariable("CONNECTION_STRING", ConnectionString.TestDatabase());
            await ElasticsearchTests.BeforeAnyElasticsearchTest(ElasticsearchClient).ConfigureAwait(true);
            try
            {
                _factory = new MockWebApplicationFactory<TStartup>(_connection);
                Client = _factory.CreateClient();
                // #region agent log
                AgentDebugLog.Write("D", "IntegrationTests.cs:BaseSetup", "WebApplicationFactory CreateClient succeeded", new
                {
                    baseAddress = Client.BaseAddress?.ToString()
                });
                // #endregion
                DatabaseContext = new AddressesContext(_builder.Options);
                DatabaseContext.Database.Migrate();
                _transaction = DatabaseContext.Database.BeginTransaction();
                // #region agent log
                AgentDebugLog.Write("D", "IntegrationTests.cs:BaseSetup", "factory and migrate succeeded", new { run = "post-fix" });
                // #endregion
            }
            catch (Exception ex)
            {
                // #region agent log
                AgentDebugLog.Write("D", "IntegrationTests.cs:BaseSetup", "factory or migrate failed", new
                {
                    type = ex.GetType().FullName,
                    msg = ex.Message,
                    inner = ex.InnerException?.Message
                });
                // #endregion
                throw;
            }
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            RollbackEfTransaction();
            ElasticsearchTests.DeleteAddressesIndex(ElasticsearchClient);
        }
        private void RollbackEfTransaction()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }

        private void ConnectToPostgresDbUsingEf()
        {
            try
            {
                _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
                _connection.Open();
                var npgsqlCommand = _connection.CreateCommand();
                npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
                npgsqlCommand.ExecuteNonQuery();

                _builder = new DbContextOptionsBuilder();
                _builder.UseNpgsql(_connection);
                // #region agent log
                AgentDebugLog.Write("B", "IntegrationTests.cs:ConnectToPostgresDbUsingEf", "postgres opened", new
                {
                    state = _connection.State.ToString(),
                    dbHost = Environment.GetEnvironmentVariable("DB_HOST")
                });
                // #endregion
            }
            catch (Exception ex)
            {
                // #region agent log
                AgentDebugLog.Write("B", "IntegrationTests.cs:ConnectToPostgresDbUsingEf", "postgres open failed", new
                {
                    type = ex.GetType().FullName,
                    msg = ex.Message,
                    inner = ex.InnerException?.Message
                });
                // #endregion
                throw;
            }
        }
    }
}
