using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.HealthChecks
{
    /// <summary>
    /// بررسی وضعیت اتصال به پایگاه داده
    /// </summary>
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
                var data = new Dictionary<string, object>
                {
                    { "Database", _dbContext.Database.GetDbConnection().Database },
                    { "Server", _dbContext.Database.GetDbConnection().DataSource },
                    { "ConnectionString", MaskConnectionString(_dbContext.Database.GetConnectionString()) }
                };

                // بررسی اتصال به پایگاه داده
                if (await _dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    // بررسی زمان پاسخ پایگاه داده
                    var startTime = DateTime.UtcNow;
                    await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                    var responseTime = DateTime.UtcNow - startTime;

                    data.Add("ResponseTime", responseTime.TotalMilliseconds);

                    if (responseTime.TotalMilliseconds > 1000) // بیش از 1 ثانیه
                    {
                        _logger.LogWarning("زمان پاسخ پایگاه داده بالا است: {ResponseTime}ms", responseTime.TotalMilliseconds);
                        return new HealthCheckResult(
                            status: HealthStatus.Degraded,
                            description: "زمان پاسخ پایگاه داده بالا است",
                            data: data);
                    }

                    return new HealthCheckResult(
                        status: HealthStatus.Healthy,
                        description: "اتصال به پایگاه داده برقرار است",
                        data: data);
                }

                _logger.LogError("عدم توانایی در اتصال به پایگاه داده");
                return new HealthCheckResult(
                    status: HealthStatus.Unhealthy,
                    description: "عدم توانایی در اتصال به پایگاه داده",
                    data: data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وضعیت پایگاه داده");
                return new HealthCheckResult(
                    status: HealthStatus.Unhealthy,
                    description: "خطا در بررسی وضعیت پایگاه داده",
                    exception: ex);
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return string.Empty;

            // پنهان کردن اطلاعات حساس در رشته اتصال
            var masked = connectionString;
            var sensitiveKeys = new[] { "Password", "Pwd", "User ID", "UID" };

            foreach (var key in sensitiveKeys)
            {
                var pattern = $"{key}=[^;]*";
                masked = System.Text.RegularExpressions.Regex.Replace(masked, pattern, $"{key}=*****");
            }

            return masked;
        }
    }
}