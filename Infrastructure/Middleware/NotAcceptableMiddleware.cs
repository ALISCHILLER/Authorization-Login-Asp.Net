using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class NotAcceptableMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<NotAcceptableMiddleware> _logger;

        public NotAcceptableMiddleware(RequestDelegate next, ILogger<NotAcceptableMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status406NotAcceptable)
            {
                var acceptHeader = context.Request.Headers["Accept"].ToString();
                var contentType = context.Response.ContentType;

                _logger.LogWarning(
                    "درخواست با نوع محتوای نامعتبر: Accept={AcceptHeader}, ContentType={ContentType}, Path={Path}",
                    acceptHeader, contentType, context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "NotAcceptable",
                        message = "نوع محتوای درخواستی پشتیبانی نمی‌شود",
                        acceptHeader = acceptHeader,
                        contentType = contentType,
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