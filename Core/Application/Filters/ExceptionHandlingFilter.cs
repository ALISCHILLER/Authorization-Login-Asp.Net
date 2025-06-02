using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر برای مدیریت متمرکز خطاها
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionHandlingFilterAttribute : Attribute, IAsyncExceptionFilter
    {
        private readonly ILogger<ExceptionHandlingFilterAttribute> _logger;

        public ExceptionHandlingFilterAttribute(ILogger<ExceptionHandlingFilterAttribute> logger)
        {
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;
            var statusCode = GetStatusCode(exception);
            var message = GetMessage(exception);
            var details = GetDetails(exception);

            _logger.LogError(exception, "An unhandled exception occurred: {Message}", message);

            var response = new
            {
                StatusCode = statusCode,
                Message = message,
                Details = details,
                RequestId = context.HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            context.Result = new JsonResult(response)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;

            // تنظیم هدرهای پاسخ
            context.HttpContext.Response.Headers.Add("X-Error-Type", exception.GetType().Name);
            context.HttpContext.Response.Headers.Add("X-Request-ID", context.HttpContext.TraceIdentifier);

            await Task.CompletedTask;
        }

        private int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ForbiddenException => StatusCodes.Status403Forbidden,
                ConflictException => StatusCodes.Status409Conflict,
                DomainException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private string GetMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => validationException.Message,
                NotFoundException notFoundException => notFoundException.Message,
                UnauthorizedAccessException => "دسترسی غیرمجاز",
                ForbiddenException forbiddenException => forbiddenException.Message,
                ConflictException conflictException => conflictException.Message,
                DomainException domainException => domainException.Message,
                _ => "خطای داخلی سرور"
            };
        }

        private object GetDetails(Exception exception)
        {
            return exception switch
            {
                ValidationException validationException => validationException.Errors,
                DomainException domainException => domainException.Details,
                _ => null
            };
        }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
} 