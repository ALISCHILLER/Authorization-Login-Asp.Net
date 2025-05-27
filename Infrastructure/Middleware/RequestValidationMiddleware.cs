using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestValidationMiddleware> _logger;
        private readonly MiddlewareConfiguration _config;

        public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger, MiddlewareConfiguration config)
        {
            _next = next;
            _logger = logger;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // بررسی اندازه درخواست
                if (context.Request.ContentLength > _config.MaxRequestSize)
                {
                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    await context.Response.WriteAsJsonAsync(new { error = "حجم درخواست بیش از حد مجاز است" });
                    return;
                }

                // بررسی نوع محتوا برای درخواست‌های POST و PUT
                if (context.Request.Method == "POST" || context.Request.Method == "PUT")
                {
                    var contentType = context.Request.ContentType?.ToLower();
                    if (string.IsNullOrEmpty(contentType) || !contentType.Contains("application/json"))
                    {
                        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                        await context.Response.WriteAsJsonAsync(new { error = "نوع محتوای درخواست باید application/json باشد" });
                        return;
                    }
                }

                // بررسی هدرهای امنیتی
                if (!ValidateSecurityHeaders(context))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "هدرهای امنیتی نامعتبر" });
                    return;
                }

                // بررسی پارامترهای URL
                if (!ValidateQueryParameters(context))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "پارامترهای URL نامعتبر" });
                    return;
                }

                await _next(context);
            }
            catch (JsonException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "فرمت JSON نامعتبر" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اعتبارسنجی درخواست");
                throw;
            }
        }

        private bool ValidateSecurityHeaders(HttpContext context)
        {
            // بررسی CSRF توکن برای درخواست‌های POST، PUT و DELETE
            if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE")
            {
                var csrfToken = context.Request.Headers["X-XSRF-TOKEN"].ToString();
                if (string.IsNullOrEmpty(csrfToken))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateQueryParameters(HttpContext context)
        {
            foreach (var param in context.Request.Query)
            {
                // بررسی طول پارامترها
                if (param.Value.ToString().Length > _config.MaxQueryStringLength)
                {
                    return false;
                }

                // بررسی کاراکترهای غیرمجاز
                if (param.Value.ToString().Any(c => !char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.'))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task LogRequest(HttpContext context)
        {
            var request = context.Request;
            var requestBody = string.Empty;

            if (request.Body.CanRead)
            {
                request.EnableBuffering();
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }
            }

            _logger.LogInformation(
                "Request: {Method} {Path} {QueryString} {Body}",
                request.Method,
                request.Path,
                request.QueryString,
                requestBody);
        }
    }
} 