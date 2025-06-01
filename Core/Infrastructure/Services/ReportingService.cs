using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public interface IReportingService
    {
        Task<byte[]> GenerateSystemReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateUserActivityReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateSecurityReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateErrorReportAsync(DateTime startDate, DateTime endDate);
    }

    public class ReportingService : IReportingService
    {
        private readonly ILogger<ReportingService> _logger;
        private readonly IMetricsService _metricsService;
        private readonly IHealthCheckService _healthCheckService;

        public ReportingService(
            ILogger<ReportingService> logger,
            IMetricsService metricsService,
            IHealthCheckService healthCheckService)
        {
            _logger = logger;
            _metricsService = metricsService;
            _healthCheckService = healthCheckService;
        }

        public async Task<byte[]> GenerateSystemReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                var healthStatus = await _healthCheckService.CheckHealthAsync();

                var report = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { StartDate = startDate, EndDate = endDate },
                    SystemStatus = new
                    {
                        HealthStatus = healthStatus.Status.ToString(),
                        Components = healthStatus.Entries.Select(e => new
                        {
                            Name = e.Key,
                            Status = e.Value.Status.ToString(),
                            e.Value.Description
                        })
                    },
                    PerformanceMetrics = new
                    {
                        TotalRequests = metrics.EndpointMetrics.Values.Sum(m => m.TotalRequests),
                        ErrorRate = metrics.EndpointMetrics.Values.Sum(m => m.ErrorCount) /
                                  (double)metrics.EndpointMetrics.Values.Sum(m => m.TotalRequests),
                        AverageResponseTime = metrics.EndpointMetrics.Values.Average(m => m.AverageResponseTime)
                    },
                    metrics.CacheMetrics,
                    metrics.DatabaseMetrics,
                    metrics.ExternalServiceMetrics
                };

                return GeneratePdfReport(report, "System Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating system report");
                throw;
            }
        }

        public async Task<byte[]> GenerateUserActivityReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                var userActivity = metrics.EndpointMetrics
                    .Where(m => m.Key.Contains("user", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var report = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { StartDate = startDate, EndDate = endDate },
                    UserActivity = new
                    {
                        TotalRequests = userActivity.Values.Sum(m => m.TotalRequests),
                        ErrorCount = userActivity.Values.Sum(m => m.ErrorCount),
                        AverageResponseTime = userActivity.Values.Average(m => m.AverageResponseTime),
                        Endpoints = userActivity.Select(e => new
                        {
                            Endpoint = e.Key,
                            Requests = e.Value.TotalRequests,
                            Errors = e.Value.ErrorCount,
                            e.Value.AverageResponseTime
                        })
                    }
                };

                return GeneratePdfReport(report, "User Activity Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user activity report");
                throw;
            }
        }

        public async Task<byte[]> GenerateSecurityReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                var securityMetrics = metrics.EndpointMetrics
                    .Where(m => m.Key.Contains("auth", StringComparison.OrdinalIgnoreCase) ||
                               m.Key.Contains("security", StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var report = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { StartDate = startDate, EndDate = endDate },
                    SecurityMetrics = new
                    {
                        TotalAuthRequests = securityMetrics.Values.Sum(m => m.TotalRequests),
                        FailedAuthAttempts = securityMetrics.Values.Sum(m => m.ErrorCount),
                        SuccessRate = 1 - securityMetrics.Values.Sum(m => m.ErrorCount) /
                                         (double)securityMetrics.Values.Sum(m => m.TotalRequests),
                        Endpoints = securityMetrics.Select(e => new
                        {
                            Endpoint = e.Key,
                            Requests = e.Value.TotalRequests,
                            Errors = e.Value.ErrorCount,
                            e.Value.AverageResponseTime
                        })
                    }
                };

                return GeneratePdfReport(report, "Security Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating security report");
                throw;
            }
        }

        public async Task<byte[]> GeneratePerformanceReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                var report = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { StartDate = startDate, EndDate = endDate },
                    PerformanceMetrics = new
                    {
                        ResponseTimes = metrics.EndpointMetrics
                            .OrderByDescending(m => m.Value.AverageResponseTime)
                            .Take(10)
                            .Select(e => new
                            {
                                Endpoint = e.Key,
                                e.Value.AverageResponseTime,
                                e.Value.TotalRequests
                            }),
                        CachePerformance = new
                        {
                            HitRate = metrics.CacheMetrics.GetValueOrDefault("hits", 0) /
                                    (double)(metrics.CacheMetrics.GetValueOrDefault("hits", 0) +
                                            metrics.CacheMetrics.GetValueOrDefault("misses", 0)),
                            TotalHits = metrics.CacheMetrics.GetValueOrDefault("hits", 0),
                            TotalMisses = metrics.CacheMetrics.GetValueOrDefault("misses", 0)
                        },
                        DatabasePerformance = metrics.DatabaseMetrics,
                        ExternalServicePerformance = metrics.ExternalServiceMetrics
                    }
                };

                return GeneratePdfReport(report, "Performance Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating performance report");
                throw;
            }
        }

        public async Task<byte[]> GenerateErrorReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var metrics = _metricsService.GetMetricsSnapshot();
                var errorMetrics = metrics.EndpointMetrics
                    .Where(m => m.Value.ErrorCount > 0)
                    .OrderByDescending(m => m.Value.ErrorCount)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var report = new
                {
                    GeneratedAt = DateTime.UtcNow,
                    Period = new { StartDate = startDate, EndDate = endDate },
                    ErrorMetrics = new
                    {
                        TotalErrors = errorMetrics.Values.Sum(m => m.ErrorCount),
                        ErrorRate = errorMetrics.Values.Sum(m => m.ErrorCount) /
                                  (double)metrics.EndpointMetrics.Values.Sum(m => m.TotalRequests),
                        Endpoints = errorMetrics.Select(e => new
                        {
                            Endpoint = e.Key,
                            e.Value.ErrorCount,
                            ErrorRate = e.Value.ErrorCount / (double)e.Value.TotalRequests,
                            e.Value.TotalRequests
                        })
                    }
                };

                return GeneratePdfReport(report, "Error Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating error report");
                throw;
            }
        }

        private byte[] GeneratePdfReport(object data, string title)
        {
            // در اینجا می‌توانید از کتابخانه‌های مختلف برای تولید PDF استفاده کنید
            // برای مثال: iTextSharp, DinkToPdf, QuestPDF و غیره
            // این یک پیاده‌سازی ساده است که JSON را برمی‌گرداند
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return Encoding.UTF8.GetBytes(json);
        }
    }
}