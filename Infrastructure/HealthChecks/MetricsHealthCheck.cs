using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Infrastructure.Services;

namespace Authorization_Login_Asp.Net.Infrastructure.HealthChecks
{
    /// <summary>
    /// بررسی وضعیت متریک‌های برنامه
    /// </summary>
    public class MetricsHealthCheck : IHealthCheck
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsHealthCheck> _logger;
        private readonly double _errorRateThreshold = 0.1; // 10% نرخ خطا
        private readonly long _responseTimeThreshold = 1000; // 1 ثانیه

        public MetricsHealthCheck(IMetricsService metricsService, ILogger<MetricsHealthCheck> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var snapshot = _metricsService.GetMetricsSnapshot();
                var data = new Dictionary<string, object>();
                var unhealthyEndpoints = new List<string>();
                var degradedEndpoints = new List<string>();

                foreach (var endpoint in snapshot.EndpointMetrics)
                {
                    var metrics = endpoint.Value;
                    var errorRate = metrics.TotalRequests > 0 
                        ? (double)metrics.ErrorCount / metrics.TotalRequests 
                        : 0;

                    var endpointData = new Dictionary<string, object>
                    {
                        { "TotalRequests", metrics.TotalRequests },
                        { "ErrorCount", metrics.ErrorCount },
                        { "ErrorRate", errorRate },
                        { "AverageResponseTime", metrics.AverageResponseTime }
                    };

                    data[$"Endpoint_{endpoint.Key}"] = endpointData;

                    if (errorRate >= _errorRateThreshold)
                    {
                        unhealthyEndpoints.Add(endpoint.Key);
                        _logger.LogWarning("نرخ خطای بالا برای اندپوینت {Endpoint}: {ErrorRate:P2}", 
                            endpoint.Key, errorRate);
                    }
                    else if (metrics.AverageResponseTime > _responseTimeThreshold)
                    {
                        degradedEndpoints.Add(endpoint.Key);
                        _logger.LogWarning("زمان پاسخ بالا برای اندپوینت {Endpoint}: {ResponseTime}ms", 
                            endpoint.Key, metrics.AverageResponseTime);
                    }
                }

                // اضافه کردن متریک‌های کش
                data["Cache"] = new Dictionary<string, object>
                {
                    { "Hits", snapshot.CacheMetrics.GetValueOrDefault("hits", 0) },
                    { "Misses", snapshot.CacheMetrics.GetValueOrDefault("misses", 0) }
                };

                // اضافه کردن متریک‌های پایگاه داده
                data["Database"] = snapshot.DatabaseMetrics;

                // اضافه کردن متریک‌های سرویس‌های خارجی
                data["ExternalServices"] = snapshot.ExternalServiceMetrics;

                if (unhealthyEndpoints.Count > 0)
                {
                    return Task.FromResult(new HealthCheckResult(
                        status: HealthStatus.Unhealthy,
                        description: $"نرخ خطای بالا در اندپوینت‌های: {string.Join(", ", unhealthyEndpoints)}",
                        data: data));
                }

                if (degradedEndpoints.Count > 0)
                {
                    return Task.FromResult(new HealthCheckResult(
                        status: HealthStatus.Degraded,
                        description: $"زمان پاسخ بالا در اندپوینت‌های: {string.Join(", ", degradedEndpoints)}",
                        data: data));
                }

                return Task.FromResult(new HealthCheckResult(
                    status: HealthStatus.Healthy,
                    description: "وضعیت متریک‌های برنامه مناسب است",
                    data: data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت متریک‌ها");
                return Task.FromResult(new HealthCheckResult(
                    status: HealthStatus.Unhealthy,
                    description: "خطا در بررسی وضعیت متریک‌ها",
                    exception: ex));
            }
        }
    }
} 