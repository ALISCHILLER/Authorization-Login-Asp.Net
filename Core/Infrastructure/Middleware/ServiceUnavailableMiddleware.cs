using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class ServiceUnavailableMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ServiceUnavailableMiddleware> _logger;

        public ServiceUnavailableMiddleware(RequestDelegate next, ILogger<ServiceUnavailableMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status503ServiceUnavailable)
            {
                var serviceName = context.Items["ServiceName"]?.ToString() ?? "Unknown";
                var failureReason = context.Items["FailureReason"]?.ToString() ?? "Unknown";

                _logger.LogError(
                    "سرویس در دسترس نیست: Service={Service}, Reason={Reason}, Path={Path}",
                    serviceName, failureReason, context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "ServiceUnavailable",
                        message = "سرویس در حال حاضر در دسترس نیست",
                        service = serviceName,
                        reason = failureReason,
                        path = context.Request.Path.ToString(),
                        retryAfter = context.Response.Headers["Retry-After"].ToString(),
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}