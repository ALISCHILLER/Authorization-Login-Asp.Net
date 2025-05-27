namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class MiddlewareConfiguration
    {
        public int MaxRequestSize { get; set; } = 10 * 1024 * 1024; // 10MB
        public int MaxQueryStringLength { get; set; } = 2048; // 2KB
        public bool EnableRequestLogging { get; set; } = true;
        public bool EnableResponseLogging { get; set; } = true;
        public bool EnableAuditLogging { get; set; } = true;
        public bool EnableSecurityHeaders { get; set; } = true;
        public bool EnableRequestValidation { get; set; } = true;
        public bool EnableResponseCompression { get; set; } = true;
        public bool EnableExceptionHandling { get; set; } = true;
        public bool EnableRateLimiting { get; set; } = true;
        public bool EnableCors { get; set; } = true;
        public bool EnableHsts { get; set; } = true;
        public bool EnableAntiforgery { get; set; } = true;
    }
} 