using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Authorization_Login_Asp.Net.Application.Exceptions;

namespace Authorization_Login_Asp.Net.API.Middlewares
{
    /// <summary>
    /// Middleware برای مدیریت استثناها و برگرداندن پاسخ‌های مناسب HTTP
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // اجرای درخواست بعدی در Pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // گرفتن خطا و پاسخ مناسب دادن
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // پیش‌فرض: کد وضعیت 500 (خطای داخلی سرور)
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string message = "خطایی در سرور رخ داد.";

            // تعیین کد وضعیت و پیام بر اساس نوع استثنا
            switch (exception)
            {
                case NotFoundException:
                    statusCode = HttpStatusCode.NotFound; // 404
                    message = exception.Message;
                    break;
                case UnauthorizedException:
                    statusCode = HttpStatusCode.Unauthorized; // 401
                    message = exception.Message;
                    break;
                case ForbiddenException:
                    statusCode = HttpStatusCode.Forbidden; // 403
                    message = exception.Message;
                    break;
                case BadRequestException:
                    statusCode = HttpStatusCode.BadRequest; // 400
                    message = exception.Message;
                    break;
                default:
                    // لاگ کردن خطاهای غیرمنتظره برای بررسی در لاگ‌ها
                    _logger.LogError(exception, "خطای ناشناخته رخ داد");
                    break;
            }

            // ساختار پاسخ JSON
            var response = new
            {
                StatusCode = (int)statusCode,
                ErrorMessage = message
            };

            string jsonResponse = JsonConvert.SerializeObject(response);

            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
