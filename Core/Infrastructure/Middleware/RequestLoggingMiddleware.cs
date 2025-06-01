using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;

                var logMessage = new
                {
                    context.Request.Method,
                    context.Request.Path,
                    QueryString = context.Request.QueryString.ToString(),
                    context.Response.StatusCode,
                    ElapsedMilliseconds = elapsed,
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    IP = context.Connection.RemoteIpAddress?.ToString(),
                    User = context.User.Identity?.Name
                };

                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                              context.Response.StatusCode >= 400 ? LogLevel.Warning :
                              LogLevel.Information;

                _logger.Log(logLevel, "درخواست HTTP {Method} {Path} با وضعیت {StatusCode} در {ElapsedMilliseconds}ms",
                    logMessage.Method,
                    logMessage.Path,
                    logMessage.StatusCode,
                    logMessage.ElapsedMilliseconds);
            }
        }
    }
}