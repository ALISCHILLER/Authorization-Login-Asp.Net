using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface IMetricsService
    {
        void IncrementRequestCount(string endpoint);
        void IncrementErrorCount(string endpoint);
        void RecordResponseTime(string endpoint, long milliseconds);
        void RecordCacheHit(string cacheKey);
        void RecordCacheMiss(string cacheKey);
        void RecordDatabaseOperation(string operation, long milliseconds);
        void RecordExternalServiceCall(string service, long milliseconds);
        MetricsSnapshot GetMetricsSnapshot();
    }

    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly ConcurrentDictionary<string, EndpointMetrics> _endpointMetrics = new();
        private readonly ConcurrentDictionary<string, long> _cacheMetrics = new();
        private readonly ConcurrentDictionary<string, long> _databaseMetrics = new();
        private readonly ConcurrentDictionary<string, long> _externalServiceMetrics = new();
        private readonly object _lockObject = new();

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        public void IncrementRequestCount(string endpoint)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            Interlocked.Increment(ref metrics.TotalRequests);
        }

        public void IncrementErrorCount(string endpoint)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            Interlocked.Increment(ref metrics.ErrorCount);
        }

        public void RecordResponseTime(string endpoint, long milliseconds)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            lock (_lockObject)
            {
                metrics.TotalResponseTime += milliseconds;
                metrics.AverageResponseTime = metrics.TotalResponseTime / metrics.TotalRequests;
            }
        }

        public void RecordCacheHit(string cacheKey)
        {
            Interlocked.Increment(ref _cacheMetrics.GetOrAdd("hits", 0));
        }

        public void RecordCacheMiss(string cacheKey)
        {
            Interlocked.Increment(ref _cacheMetrics.GetOrAdd("misses", 0));
        }

        public void RecordDatabaseOperation(string operation, long milliseconds)
        {
            _databaseMetrics.AddOrUpdate(operation, milliseconds, (_, oldValue) => oldValue + milliseconds);
        }

        public void RecordExternalServiceCall(string service, long milliseconds)
        {
            _externalServiceMetrics.AddOrUpdate(service, milliseconds, (_, oldValue) => oldValue + milliseconds);
        }

        public MetricsSnapshot GetMetricsSnapshot()
        {
            return new MetricsSnapshot
            {
                EndpointMetrics = _endpointMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                CacheMetrics = _cacheMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                DatabaseMetrics = _databaseMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ExternalServiceMetrics = _externalServiceMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class EndpointMetrics
    {
        public long TotalRequests;
        public long ErrorCount;
        public long TotalResponseTime;
        public double AverageResponseTime;
    }

    public class MetricsSnapshot
    {
        public Dictionary<string, EndpointMetrics> EndpointMetrics { get; set; }
        public Dictionary<string, long> CacheMetrics { get; set; }
        public Dictionary<string, long> DatabaseMetrics { get; set; }
        public Dictionary<string, long> ExternalServiceMetrics { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 