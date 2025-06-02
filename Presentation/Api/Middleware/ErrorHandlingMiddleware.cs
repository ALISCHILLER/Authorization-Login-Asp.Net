using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            IErrorHandlingService errorHandlingService,
            ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _errorHandlingService = errorHandlingService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, context);
            }
        }

        private async Task HandleExceptionAsync(Exception ex, HttpContext context)
        {
            var userId = context.User?.FindFirst("sub")?.Value;
            var statusCode = GetStatusCode(ex);
            var message = GetUserFriendlyMessage(ex);

            // ثبت خطا
            if (userId != null)
            {
                await _errorHandlingService.LogUserErrorAsync(userId, message, ex);
            }
            else
            {
                await _errorHandlingService.LogSystemErrorAsync("API", message, ex);
            }

            _logger.LogError(ex, "خطا در پردازش درخواست: {Message}", message);

            // ارسال پاسخ به کاربر
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = new
                {
                    message = message,
                    statusCode = statusCode
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }

        private static int GetStatusCode(Exception ex) => ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetUserFriendlyMessage(Exception ex) => ex switch
        {
            UnauthorizedAccessException => "دسترسی غیرمجاز",
            ArgumentException => "اطلاعات ورودی نامعتبر است",
            InvalidOperationException => "عملیات نامعتبر است",
            KeyNotFoundException => "مورد درخواستی یافت نشد",
            _ => "خطای سیستمی رخ داده است"
        };
    }
} 