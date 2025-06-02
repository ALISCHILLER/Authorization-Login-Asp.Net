using Authorization_Login_Asp.Net.Core.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Middleware
{
    /// <summary>
    /// میدلور مدیریت خطاهای برنامه
    /// این میدلور تمام خطاهای برنامه را مدیریت کرده و پاسخ مناسب را به کلاینت برمی‌گرداند
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// سازنده میدلور مدیریت خطا
        /// </summary>
        /// <param name="next">دلیگیت درخواست بعدی</param>
        /// <param name="logger">لاگر</param>
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// پردازش درخواست و مدیریت خطاها
        /// </summary>
        /// <param name="context">کانتکست درخواست HTTP</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// مدیریت خطا و ارسال پاسخ مناسب به کلاینت
        /// </summary>
        /// <param name="context">کانتکست درخواست HTTP</param>
        /// <param name="exception">خطای رخ داده</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Message = "خطای سیستمی رخ داده است"
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "خطای اعتبارسنجی";
                    errorResponse.Errors = validationEx.Errors;
                    _logger.LogWarning(exception, "خطای اعتبارسنجی در درخواست {RequestPath}", context.Request.Path);
                    break;

                case NotFoundException notFoundEx:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = notFoundEx.Message;
                    _logger.LogWarning(exception, "منبع مورد نظر یافت نشد: {Message}", notFoundEx.Message);
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = "دسترسی غیرمجاز";
                    _logger.LogWarning(exception, "دسترسی غیرمجاز به {RequestPath}", context.Request.Path);
                    break;

                case ForbiddenAccessException:
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Message = "دسترسی ممنوع";
                    _logger.LogWarning(exception, "دسترسی ممنوع به {RequestPath}", context.Request.Path);
                    break;

                case ConflictException conflictEx:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = conflictEx.Message;
                    _logger.LogWarning(exception, "تعارض در درخواست: {Message}", conflictEx.Message);
                    break;

                case BusinessRuleException businessEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = businessEx.Message;
                    _logger.LogWarning(exception, "خطای قوانین کسب و کار: {Message}", businessEx.Message);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(exception, "خطای ناشناخته در پردازش درخواست {RequestPath}", context.Request.Path);
                    break;
            }

            var result = JsonSerializer.Serialize(errorResponse, _jsonOptions);
            await response.WriteAsync(result);
        }
    }

    /// <summary>
    /// مدل پاسخ خطا
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// شناسه ردیابی درخواست
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// پیام خطا
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// لیست خطاهای اعتبارسنجی
        /// </summary>
        public IDictionary<string, string[]> Errors { get; set; }
    }

    /// <summary>
    /// استثنای مورد یافت نشد
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
} 