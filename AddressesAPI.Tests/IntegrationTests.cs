using System;
using System.IO;
using System.Net.Http;
using AddressesAPI.V1.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    public class IntegrationTests<TStartup> where TStartup : class
    {
        protected HttpClient Client { get; private set; }
        protected AddressesContext DatabaseContext { get; private set; }
        protected SqlConnection Db { get; private set; }

        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;
        private string _connectionString;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Environment.SetEnvironmentVariable("ALLOWED_ADDRESSSTATUS_VALUES", "historical;alternative;approved preferred;provisional");
            // ConnectToPostgresDbUsingEf();
        }

        [SetUp]
        public void BaseSetup()
        {
            ConnectToDbUsingSqlClient();
            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            Client = _factory.CreateClient();
            // StartTransactionWithEf();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            // RollbackEfTransaction();
        }



        [OneTimeTearDown]
        public void AfterAllTests()
        {
            Db.Close();
            Db.Dispose();
        }

        private void StartTransactionWithEf()
        {
            DatabaseContext = new AddressesContext(_builder.Options);
            DatabaseContext.Database.EnsureCreated();
            _transaction = DatabaseContext.Database.BeginTransaction();
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


        private void ConnectToDbUsingSqlClient()
        {
            _connectionString = Environment.GetEnvironmentVariable("LLPGConnectionString");
            if (_connectionString == null)
            {
                var dotenv = Path.GetRelativePath(Directory.GetCurrentDirectory(), "../../../../database.env");
                DotNetEnv.Env.Load(dotenv);
                _connectionString = DotNetEnv.Env.GetString("LLPGConnectionString");
            }
            Environment.SetEnvironmentVariable("LLPGConnectionString", _connectionString);

            Db = new SqlConnection(_connectionString);
            Db.Open();
        }
    }

}
