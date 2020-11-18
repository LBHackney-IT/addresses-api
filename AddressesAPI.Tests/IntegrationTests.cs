using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AddressesAPI.Infrastructure;
using AddressesAPI.V1.Boundary.Responses;
using AddressesAPI.V1.Boundary.Responses.Metadata;
using Elasticsearch.Net;
using FluentAssertions.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Nest;
using Newtonsoft.Json;
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
        private string _esDomainUri = "http://localhost:9202";
        private ElasticsearchTests _elasticserachTests;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _elasticserachTests = new ElasticsearchTests();
            ConnectToPostgresDbUsingEf();
        }

        [SetUp]
        public void BaseSetup()
        {
            Environment.SetEnvironmentVariable("CONNECTION_STRING", ConnectionString.TestDatabase());
            Environment.SetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL", _esDomainUri);
            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            Client = _factory.CreateClient();
            DatabaseContext = new AddressesContext(_builder.Options);
            DatabaseContext.Database.Migrate();
            _transaction = DatabaseContext.Database.BeginTransaction();
            ElasticsearchClient = _elasticserachTests.SetupElasticsearchConnection();
            ElasticsearchTests.DeleteAddressesIndex(ElasticsearchClient);
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
            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();
            var npgsqlCommand = _connection.CreateCommand();
            npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            npgsqlCommand.ExecuteNonQuery();

            _builder = new DbContextOptionsBuilder();
            _builder.UseNpgsql(_connection);
        }
    }
}
