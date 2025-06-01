using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    /// <summary>
    /// میدلور محدودیت نرخ درخواست
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimiter _rateLimiter;
        private readonly IConfiguration _configuration;

        public RateLimitingMiddleware(
            RequestDelegate next,
            RateLimiter rateLimiter,
            IConfiguration configuration)
        {
            _next = next;
            _rateLimiter = rateLimiter;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // فقط برای درخواست‌های POST مربوط به احراز هویت اعمال می‌شود
            if (context.Request.Method == "POST" &&
                (context.Request.Path.StartsWithSegments("/api/auth/login") ||
                 context.Request.Path.StartsWithSegments("/api/auth/register")))
            {
                var key = context.Connection.RemoteIpAddress?.ToString();
                if (string.IsNullOrEmpty(key))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var maxAttempts = _configuration.GetValue("Security:RateLimiting:MaxAttempts", 5);
                var windowMinutes = _configuration.GetValue("Security:RateLimiting:WindowMinutes", 15);

                if (!await _rateLimiter.CheckAndIncrementAsync(key, maxAttempts, windowMinutes))
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers.Add("Retry-After", windowMinutes.ToString());
                    return;
                }
            }

            await _next(context);
        }
    }
}