using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class EnhancedLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnhancedLoggingMiddleware> _logger;

        public EnhancedLoggingMiddleware(RequestDelegate next, ILogger<EnhancedLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var requestId = context.TraceIdentifier;
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString() ?? Guid.NewGuid().ToString();

            // اضافه کردن شناسه‌های ردیابی به هدرهای پاسخ
            context.Response.Headers.Add("X-Request-ID", requestId);
            context.Response.Headers.Add("X-Correlation-ID", correlationId);

            try
            {
                // لاگ کردن اطلاعات درخواست
                LogRequest(context, requestId, correlationId);

                await _next(context);

                sw.Stop();

                // لاگ کردن اطلاعات پاسخ
                LogResponse(context, requestId, correlationId, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                LogException(context, ex, requestId, correlationId, sw.ElapsedMilliseconds);
                throw;
            }
        }

        private void LogRequest(HttpContext context, string requestId, string correlationId)
        {
            var requestInfo = new
            {
                RequestId = requestId,
                CorrelationId = correlationId,
                context.Request.Method,
                Path = context.Request.Path.ToString(),
                QueryString = context.Request.QueryString.ToString(),
                Headers = context.Request.Headers
                    .Where(h => !h.Key.StartsWith("Authorization"))
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                ClientIP = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                User = context.User.Identity?.Name,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation(
                "درخواست دریافت شد: {RequestInfo}",
                JsonSerializer.Serialize(requestInfo));
        }

        private void LogResponse(HttpContext context, string requestId, string correlationId, long elapsedMs)
        {
            var responseInfo = new
            {
                RequestId = requestId,
                CorrelationId = correlationId,
                context.Response.StatusCode,
                context.Response.ContentType,
                Headers = context.Response.Headers
                    .ToDictionary(h => h.Key, h => h.Value.ToString()),
                ElapsedMilliseconds = elapsedMs,
                Timestamp = DateTime.UtcNow
            };

            var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            _logger.Log(
                logLevel,
                "پاسخ ارسال شد: {ResponseInfo}",
                JsonSerializer.Serialize(responseInfo));
        }

        private void LogException(HttpContext context, Exception ex, string requestId, string correlationId, long elapsedMs)
        {
            var exceptionInfo = new
            {
                RequestId = requestId,
                CorrelationId = correlationId,
                ExceptionType = ex.GetType().Name,
                ex.Message,
                ex.StackTrace,
                Path = context.Request.Path.ToString(),
                context.Request.Method,
                ElapsedMilliseconds = elapsedMs,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogError(
                ex,
                "خطا در پردازش درخواست: {ExceptionInfo}",
                JsonSerializer.Serialize(exceptionInfo));
        }
    }
}