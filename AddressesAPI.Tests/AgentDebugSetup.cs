using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace AddressesAPI.Tests
{
    [SetUpFixture]
    public class AgentDebugSetup
    {
        [OneTimeSetUp]
        public void LogEnvironment()
        {
            // #region agent log
            AgentDebugLog.Write("A", "AgentDebugSetup.cs:OneTimeSetUp", "AddressesAPI.Tests assembly starting", new
            {
                framework = RuntimeInformation.FrameworkDescription,
                cwd = Environment.CurrentDirectory,
                dbHost = Environment.GetEnvironmentVariable("DB_HOST"),
                dbPort = Environment.GetEnvironmentVariable("DB_PORT"),
                dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE"),
                es = Environment.GetEnvironmentVariable("ELASTICSEARCH_DOMAIN_URL")
            });
            // #endregion
        }
    }
}
