using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class BadRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BadRequestMiddleware> _logger;

        public BadRequestMiddleware(RequestDelegate next, ILogger<BadRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status400BadRequest)
            {
                _logger.LogWarning("درخواست نامعتبر: {Path}", context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "BadRequest",
                        message = "درخواست ارسالی نامعتبر است",
                        path = context.Request.Path,
                        method = context.Request.Method,
                        validationErrors = context.Items.ContainsKey("ValidationErrors")
                            ? context.Items["ValidationErrors"]
                            : null,
                        timestamp = DateTime.UtcNow
                    }
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}