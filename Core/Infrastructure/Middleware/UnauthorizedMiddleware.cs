using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class UnauthorizedMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnauthorizedMiddleware> _logger;

        public UnauthorizedMiddleware(RequestDelegate next, ILogger<UnauthorizedMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning("دسترسی غیرمجاز: {Path}", context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "Unauthorized",
                        message = "لطفا وارد حساب کاربری خود شوید",
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