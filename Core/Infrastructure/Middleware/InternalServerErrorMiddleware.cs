using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    /// <summary>
    /// میدلور مدیریت خطاهای داخلی سرور
    /// این میدلور خطاهای پردازش نشده را مدیریت کرده و پاسخ مناسب را به کلاینت برمی‌گرداند
    /// </summary>
    public class InternalServerErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InternalServerErrorMiddleware> _logger;

        /// <summary>
        /// سازنده میدلور
        /// </summary>
        /// <param name="next">دلیگیت درخواست بعدی در خط پردازش</param>
        /// <param name="logger">سرویس لاگر</param>
        public InternalServerErrorMiddleware(RequestDelegate next, ILogger<InternalServerErrorMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// متد اصلی پردازش درخواست
        /// </summary>
        /// <param name="context">کانتکست HTTP</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // ثبت خطا در لاگ
                _logger.LogError(ex, "خطای داخلی سرور: {Message}", ex.Message);

                // تنظیم هدرهای پاسخ
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // ساخت پاسخ خطا
                var response = CreateErrorResponse(context, ex);

                // ارسال پاسخ به کلاینت
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
            }
        }

        /// <summary>
        /// ساخت پاسخ خطا بر اساس محیط اجرا
        /// </summary>
        private object CreateErrorResponse(HttpContext context, Exception ex)
        {
            var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

            // در محیط توسعه، جزئیات خطا را نمایش می‌دهیم
            if (environment.IsDevelopment())
            {
                return new
                {
                    Error = new
                    {
                        Code = "InternalServerError",
                        ex.Message,
                        ex.StackTrace,
                        RequestId = context.TraceIdentifier,
                        Timestamp = DateTime.UtcNow,
                        ex.Source,
                        InnerException = ex.InnerException?.Message
                    }
                };
            }

            // در محیط تولید، فقط اطلاعات عمومی خطا را نمایش می‌دهیم
            return new
            {
                Error = new
                {
                    Code = "InternalServerError",
                    Message = "خطای داخلی سرور رخ داده است",
                    RequestId = context.TraceIdentifier,
                    Timestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// کلاس توسعه‌دهنده برای اضافه کردن میدلور به خط پردازش درخواست
    /// </summary>
    public static class InternalServerErrorMiddlewareExtensions
    {
        /// <summary>
        /// اضافه کردن میدلور مدیریت خطای داخلی سرور به خط پردازش درخواست
        /// </summary>
        /// <param name="builder">سازنده خط پردازش درخواست</param>
        /// <returns>سازنده خط پردازش درخواست</returns>
        public static IApplicationBuilder UseInternalServerErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<InternalServerErrorMiddleware>();
        }
    }
}