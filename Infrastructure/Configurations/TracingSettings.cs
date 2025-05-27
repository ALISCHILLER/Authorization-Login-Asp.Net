namespace Authorization_Login_Asp.Net.Infrastructure.Configurations
{
    public class TracingSettings
    {
        public string ServiceName { get; set; } = "AuthService";
        public string ServiceVersion { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";
        public bool EnableTracing { get; set; } = true;
        public string JaegerEndpoint { get; set; } = "http://localhost:14268/api/traces";
        public bool EnableConsoleExporter { get; set; } = true;
        public bool EnableJaegerExporter { get; set; } = true;
        public double SamplingRatio { get; set; } = 1.0;
    }
} 