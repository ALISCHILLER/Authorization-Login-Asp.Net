using Authorization_Login_Asp.Net.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
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
        private readonly ConcurrentDictionary<string, IAsyncPolicy> _circuitBreakerPolicies;
        private readonly ConcurrentDictionary<string, IAsyncPolicy> _retryPolicies;

        public CircuitBreakerService(
            ILogger<CircuitBreakerService> logger,
            CircuitBreakerSettings settings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _circuitBreakerPolicies = new ConcurrentDictionary<string, IAsyncPolicy>();
            _retryPolicies = new ConcurrentDictionary<string, IAsyncPolicy>();
        }

        private IAsyncPolicy GetOrCreateCircuitBreakerPolicy(string serviceName)
        {
            return _circuitBreakerPolicies.GetOrAdd(serviceName, name =>
            {
                return Policy
                    .Handle<HttpRequestException>()
                    .CircuitBreakerAsync(
                        exceptionsAllowedBeforeBreaking: _settings.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: TimeSpan.FromSeconds(_settings.DurationOfBreak),
                        onBreak: (ex, duration) =>
                        {
                            _logger.LogWarning(ex, "Circuit breaker opened for {ServiceName} for {Duration} seconds",
                                name, duration.TotalSeconds);
                        },
                        onReset: () =>
                        {
                            _logger.LogInformation("Circuit breaker reset for {ServiceName}", name);
                        });
            });
        }

        private IAsyncPolicy GetOrCreateRetryPolicy(string serviceName)
        {
            return _retryPolicies.GetOrAdd(serviceName, name =>
            {
                return Policy
                    .Handle<HttpRequestException>()
                    .WaitAndRetryAsync(
                        retryCount: _settings.RetryCount,
                        sleepDurationProvider: retryAttempt =>
                            TimeSpan.FromSeconds(Math.Pow(_settings.RetryInterval, retryAttempt)),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning(exception,
                                "Retry {RetryCount} for {ServiceName} after {Delay}ms",
                                retryCount, name, timeSpan.TotalMilliseconds);
                        });
            });
        }

        public async Task<T> ExecuteWithCircuitBreakerAsync<T>(Func<Task<T>> action, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be empty", nameof(serviceName));

            if (!_settings.EnableCircuitBreaker)
            {
                return await action();
            }

            var circuitBreakerPolicy = GetOrCreateCircuitBreakerPolicy(serviceName);
            var retryPolicy = GetOrCreateRetryPolicy(serviceName);
            var context = new Context { ["ServiceName"] = serviceName };

            return await retryPolicy
                .WrapAsync(circuitBreakerPolicy)
                .ExecuteAsync(async (ctx) => await action(), context);
        }

        public async Task ExecuteWithCircuitBreakerAsync(Func<Task> action, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be empty", nameof(serviceName));

            if (!_settings.EnableCircuitBreaker)
            {
                await action();
                return;
            }

            var circuitBreakerPolicy = GetOrCreateCircuitBreakerPolicy(serviceName);
            var retryPolicy = GetOrCreateRetryPolicy(serviceName);
            var context = new Context { ["ServiceName"] = serviceName };

            await retryPolicy
                .WrapAsync(circuitBreakerPolicy)
                .ExecuteAsync(async (ctx) => await action(), context);
        }
    }
}