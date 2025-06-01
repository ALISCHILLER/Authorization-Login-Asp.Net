using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class RateLimitExceededMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitExceededMiddleware> _logger;

        public RateLimitExceededMiddleware(RequestDelegate next, ILogger<RateLimitExceededMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var endpoint = context.Request.Path;

                _logger.LogWarning(
                    "تجاوز از محدودیت نرخ درخواست: IP={IP}, UserAgent={UserAgent}, Endpoint={Endpoint}",
                    ipAddress, userAgent, endpoint);

                var response = new
                {
                    error = new
                    {
                        code = "RateLimitExceeded",
                        message = "تعداد درخواست‌های شما بیش از حد مجاز است",
                        retryAfter = context.Response.Headers["Retry-After"].ToString(),
                        ipAddress,
                        endpoint = endpoint.ToString(),
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}