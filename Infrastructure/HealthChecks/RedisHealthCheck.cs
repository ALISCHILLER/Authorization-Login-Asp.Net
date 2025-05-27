using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.HealthChecks
{
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
                // بررسی اتصال به Redis
                var db = _redis.GetDatabase();
                var pingResult = await db.PingAsync();

                if (pingResult.TotalMilliseconds > 1000)
                {
                    return HealthCheckResult.Degraded($"Redis پاسخ می‌دهد اما کند است. زمان پاسخ: {pingResult.TotalMilliseconds}ms");
                }

                return HealthCheckResult.Healthy($"Redis در دسترس است. زمان پاسخ: {pingResult.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت Redis");
                return HealthCheckResult.Unhealthy("Redis در دسترس نیست", ex);
            }
        }
    }
} 