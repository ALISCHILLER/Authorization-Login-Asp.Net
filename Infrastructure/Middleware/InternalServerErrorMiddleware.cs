using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class InternalServerErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InternalServerErrorMiddleware> _logger;

        public InternalServerErrorMiddleware(RequestDelegate next, ILogger<InternalServerErrorMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطای داخلی سرور: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    error = new
                    {
                        code = "InternalServerError",
                        message = "خطای داخلی سرور رخ داده است",
                        requestId = context.TraceIdentifier,
                        timestamp = DateTime.UtcNow
                    }
                };

                // در محیط توسعه، جزئیات خطا را هم نمایش می‌دهیم
                if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    response = new
                    {
                        error = new
                        {
                            code = "InternalServerError",
                            message = ex.Message,
                            stackTrace = ex.StackTrace,
                            requestId = context.TraceIdentifier,
                            timestamp = DateTime.UtcNow
                        }
                    };
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
} 