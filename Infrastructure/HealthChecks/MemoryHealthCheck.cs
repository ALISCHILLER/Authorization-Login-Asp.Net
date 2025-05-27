using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly ILogger<MemoryHealthCheck> _logger;
        private readonly double _warningThreshold = 0.8; // 80% استفاده از حافظه
        private readonly double _criticalThreshold = 0.9; // 90% استفاده از حافظه

        public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var memoryInfo = GC.GetGCMemoryInfo();
                var totalMemory = GC.GetTotalMemory(false);
                var maxMemory = memoryInfo.TotalAvailableMemoryBytes;
                var memoryUsage = (double)totalMemory / maxMemory;

                var data = new
                {
                    TotalMemory = totalMemory,
                    MaxMemory = maxMemory,
                    MemoryUsage = memoryUsage,
                    GCCollectionCount = GC.CollectionCount(0)
                };

                if (memoryUsage >= _criticalThreshold)
                {
                    _logger.LogWarning("استفاده از حافظه در سطح بحرانی: {MemoryUsage:P2}", memoryUsage);
                    return Task.FromResult(HealthCheckResult.Unhealthy("استفاده از حافظه در سطح بحرانی", data));
                }

                if (memoryUsage >= _warningThreshold)
                {
                    _logger.LogWarning("استفاده از حافظه در سطح هشدار: {MemoryUsage:P2}", memoryUsage);
                    return Task.FromResult(HealthCheckResult.Degraded("استفاده از حافظه در سطح هشدار", data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("وضعیت حافظه مناسب است", data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت حافظه");
                return Task.FromResult(HealthCheckResult.Unhealthy("خطا در بررسی وضعیت حافظه", ex));
            }
        }
    }
} 