using System;
using System.IO;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    [TestFixture]
    public class TestDatabaseFixture
    {
        protected SqlConnection Db { get; private set; }
        protected string ConnectionString { get; private set; }

        [SetUp]
        public void BeforeEveryTest()
        {
            ConnectionString = Environment.GetEnvironmentVariable("LLPGConnectionString");
            if (ConnectionString == null)
            {
                var dotenv = Path.GetRelativePath(Directory.GetCurrentDirectory(), "../../../../database.env");
                DotNetEnv.Env.Load(dotenv);
                ConnectionString = DotNetEnv.Env.GetString("LLPGConnectionString");
            }
            Db = new SqlConnection(ConnectionString);
            Db.Open();
        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            Db.Close();
            Db.Dispose();
        }
    }
}
