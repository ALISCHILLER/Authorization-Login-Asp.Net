using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class RequestTimeoutMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimeoutMiddleware> _logger;

        public RequestTimeoutMiddleware(RequestDelegate next, ILogger<RequestTimeoutMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                context.Response.StatusCode = StatusCodes.Status408RequestTimeout;

                var requestPath = context.Request.Path.ToString();
                var requestMethod = context.Request.Method;
                var requestStartTime = context.Items["RequestStartTime"] as DateTime? ?? DateTime.UtcNow;
                var timeoutDuration = DateTime.UtcNow - requestStartTime;

                _logger.LogWarning(
                    "درخواست با تایم‌اوت مواجه شد: Path={Path}, Method={Method}, Duration={Duration}ms",
                    requestPath, requestMethod, timeoutDuration.TotalMilliseconds);

                var response = new
                {
                    error = new
                    {
                        code = "RequestTimeout",
                        message = "زمان درخواست به پایان رسیده است",
                        path = requestPath,
                        method = requestMethod,
                        duration = timeoutDuration.TotalMilliseconds,
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}