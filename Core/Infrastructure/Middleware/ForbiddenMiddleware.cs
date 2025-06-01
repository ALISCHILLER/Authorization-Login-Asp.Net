using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class ForbiddenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ForbiddenMiddleware> _logger;

        public ForbiddenMiddleware(RequestDelegate next, ILogger<ForbiddenMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("دسترسی ممنوع: {Path} توسط کاربر {User}",
                    context.Request.Path,
                    context.User.Identity?.Name ?? "ناشناس");

                var response = new
                {
                    error = new
                    {
                        code = "Forbidden",
                        message = "شما دسترسی لازم برای انجام این عملیات را ندارید",
                        path = context.Request.Path,
                        method = context.Request.Method,
                        user = context.User.Identity?.Name,
                        roles = context.User.Claims
                            .Where(c => c.Type == "role")
                            .Select(c => c.Value),
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}