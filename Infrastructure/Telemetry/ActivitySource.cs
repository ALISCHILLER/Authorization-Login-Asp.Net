using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Infrastructure.Telemetry
{
    public static class TelemetryActivitySource
    {
        public static readonly ActivitySource Instance = new ActivitySource("Authorization.Login.Service");
    }
} 