using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface ICircuitBreakerService
    {
        Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action, string serviceName);
        Task ExecuteWithCircuitBreakerAsync(Func<Task> action, string serviceName);
    }

    public class CircuitBreakerService : ICircuitBreakerService
    {
        private readonly ILogger<CircuitBreakerService> _logger;
        private readonly CircuitBreakerSettings _settings;
        private readonly IAsyncCircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly IAsyncRetryPolicy _retryPolicy;

        public CircuitBreakerService(
            ILogger<CircuitBreakerService> logger,
            CircuitBreakerSettings settings)
        {
            _logger = logger;
            _settings = settings;

            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _settings.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromSeconds(_settings.DurationOfBreak),
                    onBreak: (ex, duration) =>
                    {
                        _logger.LogWarning(ex, "Circuit breaker opened for {ServiceName} for {Duration} seconds", 
                            serviceName, duration.TotalSeconds);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset for {ServiceName}", serviceName);
                    });

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: _settings.RetryCount,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(Math.Pow(_settings.RetryInterval, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, 
                            "Retry {RetryCount} for {ServiceName} after {Delay}ms", 
                            retryCount, context["ServiceName"], timeSpan.TotalMilliseconds);
                    });
        }

        public async Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action, string serviceName)
        {
            if (!_settings.EnableCircuitBreaker)
            {
                return await action();
            }

            var context = new Context { ["ServiceName"] = serviceName };

            return await _retryPolicy
                .WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(async () => await action(), context);
        }

        public async Task ExecuteWithCircuitBreakerAsync(Func<Task> action, string serviceName)
        {
            if (!_settings.EnableCircuitBreaker)
            {
                await action();
                return;
            }

            var context = new Context { ["ServiceName"] = serviceName };

            await _retryPolicy
                .WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(async () => await action(), context);
        }
    }
} 