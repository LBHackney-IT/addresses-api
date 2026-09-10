using AddressesAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected AddressesContext DatabaseContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            try
            {
                var builder = new DbContextOptionsBuilder();
                builder.UseNpgsql(ConnectionString.TestDatabase());
                DatabaseContext = new AddressesContext(builder.Options);

                // #region agent log
                AgentDebugLog.Write("B", "DatabaseTests.cs:RunBeforeAnyTests", "before migrate", new
                {
                    dbHost = System.Environment.GetEnvironmentVariable("DB_HOST"),
                    dbPort = System.Environment.GetEnvironmentVariable("DB_PORT"),
                    canConnect = DatabaseContext.Database.CanConnect()
                });
                // #endregion

                DatabaseContext.Database.Migrate();
                _transaction = DatabaseContext.Database.BeginTransaction();
                // #region agent log
                AgentDebugLog.Write("B", "DatabaseTests.cs:RunBeforeAnyTests", "migrate succeeded", new { run = "post-fix" });
                // #endregion
            }
            catch (System.Exception ex)
            {
                // #region agent log
                AgentDebugLog.Write("B", "DatabaseTests.cs:RunBeforeAnyTests", "postgres setup failed", new
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
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}
