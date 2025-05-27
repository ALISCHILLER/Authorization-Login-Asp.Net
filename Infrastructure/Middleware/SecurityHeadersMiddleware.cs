using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // اضافه کردن هدرهای امنیتی
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.HasStarted)
                    {
                        // هدرهای امنیتی پایه
                        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                        context.Response.Headers["X-Frame-Options"] = "DENY";
                        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                        
                        // Content Security Policy
                        context.Response.Headers["Content-Security-Policy"] = 
                            "default-src 'self'; " +
                            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                            "style-src 'self' 'unsafe-inline'; " +
                            "img-src 'self' data: https:; " +
                            "font-src 'self' data:; " +
                            "connect-src 'self'; " +
                            "frame-ancestors 'none'; " +
                            "form-action 'self'; " +
                            "base-uri 'self'; " +
                            "object-src 'none'";

                        // Permissions Policy
                        context.Response.Headers["Permissions-Policy"] = 
                            "accelerometer=(), " +
                            "camera=(), " +
                            "geolocation=(), " +
                            "gyroscope=(), " +
                            "magnetometer=(), " +
                            "microphone=(), " +
                            "payment=(), " +
                            "usb=()";

                        // HSTS
                        if (context.Request.IsHttps)
                        {
                            context.Response.Headers["Strict-Transport-Security"] = 
                                "max-age=31536000; includeSubDomains; preload";
                        }

                        // اضافه کردن هدرهای امنیتی اضافی
                        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
                        context.Response.Headers["X-Download-Options"] = "noopen";
                        context.Response.Headers["X-DNS-Prefetch-Control"] = "off";
                    }
                    return Task.CompletedTask;
                });

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اضافه کردن هدرهای امنیتی");
                throw;
            }
        }
    }
} 