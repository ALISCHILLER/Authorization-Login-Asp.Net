using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.HealthChecks
{
    /// <summary>
    /// بررسی وضعیت اتصال به Redis
    /// </summary>
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheck> _logger;

        public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "Server", _redis.GetEndPoints()[0].ToString() },
                    { "InstanceName", _redis.ClientName ?? "Unknown" }
                };

                // بررسی وضعیت اتصال
                if (!_redis.IsConnected)
                {
                    _logger.LogError("عدم اتصال به Redis");
                    return new HealthCheckResult(
                        status: HealthStatus.Unhealthy,
                        description: "عدم اتصال به Redis",
                        data: data);
                }

                // بررسی زمان پاسخ
                var startTime = DateTime.UtcNow;
                var db = _redis.GetDatabase();
                await db.PingAsync();
                var responseTime = DateTime.UtcNow - startTime;

                data.Add("ResponseTime", responseTime.TotalMilliseconds);

                // بررسی وضعیت حافظه
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var info = await server.InfoAsync("memory");
                var usedMemory = info.FirstOrDefault(x => x.Key == "used_memory_human")?.Value;
                var maxMemory = info.FirstOrDefault(x => x.Key == "maxmemory_human")?.Value;

                if (usedMemory != null)
                    data.Add("UsedMemory", usedMemory);
                if (maxMemory != null)
                    data.Add("MaxMemory", maxMemory);

                if (responseTime.TotalMilliseconds > 100) // بیش از 100 میلی‌ثانیه
                {
                    _logger.LogWarning("زمان پاسخ Redis بالا است: {ResponseTime}ms", responseTime.TotalMilliseconds);
                    return new HealthCheckResult(
                        status: HealthStatus.Degraded,
                        description: "زمان پاسخ Redis بالا است",
                        data: data);
                }

                return new HealthCheckResult(
                    status: HealthStatus.Healthy,
                    description: "اتصال به Redis برقرار است",
                    data: data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت Redis");
                return new HealthCheckResult(
                    status: HealthStatus.Unhealthy,
                    description: "خطا در بررسی وضعیت Redis",
                    exception: ex);
            }
        }
    }
}