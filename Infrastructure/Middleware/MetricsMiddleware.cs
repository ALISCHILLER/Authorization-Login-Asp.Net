using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Infrastructure.Services;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    /// <summary>
    /// میدلور جمع‌آوری متریک‌های درخواست
    /// </summary>
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMetricsService _metricsService;

        public MetricsMiddleware(RequestDelegate next, IMetricsService metricsService)
        {
            _next = next;
            _metricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _metricsService.IncrementRequestCount(endpoint);
                await _next(context);
            }
            catch
            {
                _metricsService.IncrementErrorCount(endpoint);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _metricsService.RecordResponseTime(endpoint, stopwatch.ElapsedMilliseconds);
            }
        }
    }
} 