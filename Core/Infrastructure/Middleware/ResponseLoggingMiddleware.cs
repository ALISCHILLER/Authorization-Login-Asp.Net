using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger;

        public ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                var response = await FormatResponse(context.Response);
                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                              context.Response.StatusCode >= 400 ? LogLevel.Warning :
                              LogLevel.Information;

                _logger.Log(logLevel, "پاسخ HTTP {StatusCode} برای {Path}: {Response}",
                    context.Response.StatusCode,
                    context.Request.Path,
                    response);

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لاگینگ پاسخ");
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            // اگر پاسخ خیلی بزرگ است، آن را کوتاه می‌کنیم
            if (text.Length > 1000)
            {
                text = text.Substring(0, 1000) + "...";
            }

            return text;
        }
    }
}