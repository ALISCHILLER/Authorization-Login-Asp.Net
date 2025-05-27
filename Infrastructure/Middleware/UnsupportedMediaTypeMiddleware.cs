using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class UnsupportedMediaTypeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UnsupportedMediaTypeMiddleware> _logger;

        public UnsupportedMediaTypeMiddleware(RequestDelegate next, ILogger<UnsupportedMediaTypeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status415UnsupportedMediaType)
            {
                var contentType = context.Request.ContentType;
                var acceptedTypes = context.Request.Headers["Accept"].ToString();

                _logger.LogWarning(
                    "نوع محتوای نامعتبر: ContentType={ContentType}, AcceptedTypes={AcceptedTypes}, Path={Path}",
                    contentType, acceptedTypes, context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "UnsupportedMediaType",
                        message = "نوع محتوای ارسالی پشتیبانی نمی‌شود",
                        contentType = contentType,
                        acceptedTypes = acceptedTypes.Split(',').Select(t => t.Trim()),
                        path = context.Request.Path.ToString(),
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