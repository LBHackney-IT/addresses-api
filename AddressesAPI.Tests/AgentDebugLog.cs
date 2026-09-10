using System;
using System.IO;
using System.Text.Json;

namespace AddressesAPI.Tests
{
    internal static class AgentDebugLog
    {
        public static void Write(string hypothesisId, string location, string message, object data = null)
        {
            // #region agent log
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    sessionId = "f2761d",
                    runId = Environment.GetEnvironmentVariable("AGENT_DEBUG_RUN") ?? "pre-fix",
                    hypothesisId,
                    location,
                    message,
                    data,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
                foreach (var path in new[]
                {
                    "/Users/thatgui/Documents/SoftwareEngineer/hackney/addresses-api/.cursor/debug-f2761d.log",
                    "/app/.cursor/debug-f2761d.log"
                })
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        File.AppendAllText(path, payload + Environment.NewLine);
                    }
                    catch
                    {
                        // ignore missing mount / permissions
                    }
                }
            }
            catch
            {
                // never fail tests because of debug logging
            }
            // #endregion
        }
    }
}
