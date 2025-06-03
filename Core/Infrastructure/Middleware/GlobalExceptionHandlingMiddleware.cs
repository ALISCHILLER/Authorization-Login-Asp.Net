using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware مدیریت خطای سراسری
    /// </summary>
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
        private readonly IErrorHandlingService _errorHandlingService;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger,
            IErrorHandlingService errorHandlingService)
        {
            _next = next;
            _logger = logger;
            _errorHandlingService = errorHandlingService;
        }

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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var userId = context.User?.FindFirst("sub")?.Value;
            var errorResponse = await _errorHandlingService.CreateErrorResponseAsync(exception, context);

            // ثبت خطا
            await _errorHandlingService.LogSystemErrorAsync(exception, context.Request.Path, userId);

            // تنظیم پاسخ
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.StatusCode;

            // ارسال پاسخ
            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    /// <summary>
    /// کلاس کمکی برای مدیریت خطاها
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        public static async Task<ErrorResponse> CreateErrorResponseAsync(this Exception exception, HttpContext context)
        {
            var errorResponse = new ErrorResponse
            {
                TraceId = context.TraceIdentifier,
                Path = context.Request.Path,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "خطای اعتبارسنجی";
                    errorResponse.Errors = validationEx.Errors;
                    break;

                case NotFoundException notFoundEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Message = notFoundEx.Message;
                    break;

                case UnauthorizedException unauthorizedEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.Message = unauthorizedEx.Message;
                    break;

                case ForbiddenException forbiddenEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Forbidden;
                    errorResponse.Message = forbiddenEx.Message;
                    break;

                case ConflictException conflictEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.Conflict;
                    errorResponse.Message = conflictEx.Message;
                    break;

                case BusinessException businessEx:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = businessEx.Message;
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = "خطای داخلی سرور";
                    if (context.Environment.IsDevelopment())
                    {
                        errorResponse.Details = exception.ToString();
                    }
                    break;
            }

            return errorResponse;
        }
    }

    /// <summary>
    /// مدل پاسخ خطا
    /// </summary>
    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public string Path { get; set; }
        public DateTime Timestamp { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public IDictionary<string, string[]> Errors { get; set; }
    }
}