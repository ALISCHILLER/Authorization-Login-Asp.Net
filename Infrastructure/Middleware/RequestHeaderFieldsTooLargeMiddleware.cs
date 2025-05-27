using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class RequestHeaderFieldsTooLargeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestHeaderFieldsTooLargeMiddleware> _logger;

        public RequestHeaderFieldsTooLargeMiddleware(RequestDelegate next, ILogger<RequestHeaderFieldsTooLargeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status431RequestHeaderFieldsTooLarge)
            {
                var headers = context.Request.Headers
                    .Select(h => new { Name = h.Key, Size = h.Value.ToString().Length })
                    .OrderByDescending(h => h.Size)
                    .ToList();

                var totalHeaderSize = headers.Sum(h => h.Size);
                var largestHeader = headers.FirstOrDefault();

                _logger.LogWarning(
                    "هدرهای درخواست بیش از حد بزرگ هستند: TotalSize={TotalSize}, LargestHeader={LargestHeader}, Path={Path}",
                    totalHeaderSize, largestHeader?.Name, context.Request.Path);

                var response = new
                {
                    error = new
                    {
                        code = "RequestHeaderFieldsTooLarge",
                        message = "هدرهای درخواست بیش از حد بزرگ هستند",
                        totalHeaderSize = totalHeaderSize,
                        largestHeader = largestHeader?.Name,
                        headerSizes = headers,
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