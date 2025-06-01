using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<NotFoundMiddleware> _logger;

        public NotFoundMiddleware(RequestDelegate next, ILogger<NotFoundMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 404)
            {
                _logger.LogWarning("مسیر یافت نشد: {Path}", context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "NotFound",
                        message = "مسیر درخواستی یافت نشد",
                        path = context.Request.Path,
                        method = context.Request.Method,
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}