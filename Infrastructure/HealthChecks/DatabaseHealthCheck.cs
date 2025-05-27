using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Infrastructure.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(AppDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // بررسی اتصال به دیتابیس
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    return HealthCheckResult.Healthy("دیتابیس در دسترس است.");
                }

                return HealthCheckResult.Unhealthy("دیتابیس در دسترس نیست.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت دیتابیس");
                return HealthCheckResult.Unhealthy("خطا در بررسی وضعیت دیتابیس", ex);
            }
        }
    }
} 