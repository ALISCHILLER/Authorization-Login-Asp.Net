namespace Authorization_Login_Asp.Net.Infrastructure.Configurations
{
    public class CircuitBreakerSettings
    {
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 2;
        public int DurationOfBreak { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public int RetryInterval { get; set; } = 2;
        public bool EnableCircuitBreaker { get; set; } = true;
    }
} 
 