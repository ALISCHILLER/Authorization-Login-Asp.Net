using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrGenerateCorrelationId(context);
            context.TraceIdentifier = correlationId;

            // اضافه کردن Correlation ID به هدرهای پاسخ
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[CorrelationIdHeader] = correlationId;
                return Task.CompletedTask;
            });

            // تنظیم Correlation ID برای لاگینگ
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId
            });

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پردازش درخواست با Correlation ID: {CorrelationId}", correlationId);
                throw;
            }
        }

        private string GetOrGenerateCorrelationId(HttpContext context)
        {
            // بررسی وجود Correlation ID در هدرهای درخواست
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
            {
                return correlationId.ToString();
            }

            // تولید Correlation ID جدید
            return Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }
    }
} 