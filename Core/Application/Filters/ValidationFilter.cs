using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر اعتبارسنجی و مدیریت خطاها
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter, IExceptionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var errorMessage = string.Join(", ", errors.SelectMany(e => e.Value));
                _logger.LogWarning(
                    "Validation failed for {Action} in {Controller}. Errors: {Errors}",
                    context.ActionDescriptor.DisplayName,
                    context.Controller.GetType().Name,
                    errorMessage);

                context.Result = new BadRequestObjectResult(new
                {
                    Status = 400,
                    Message = "خطا در اعتبارسنجی داده‌ها",
                    Errors = errors,
                    Timestamp = DateTime.UtcNow
                });
                return;
            }

            await next();
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var statusCode = 500;
            var message = "خطای سرور";
            object errors = null;

            switch (exception)
            {
                case ValidationException validationException:
                    statusCode = 400;
                    message = "خطا در اعتبارسنجی داده‌ها";
                    errors = validationException.Errors
                        .GroupBy(x => x.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ErrorMessage).ToArray()
                        );
                    break;

                case DomainException domainException:
                    statusCode = 400;
                    message = domainException.Message;
                    errors = new { Code = domainException.Code, Data = domainException.AdditionalData };
                    break;

                case UserDomainException userException:
                    statusCode = 400;
                    message = userException.Message;
                    errors = new { Code = userException.Code, UserId = userException.UserId, Username = userException.Username };
                    break;

                case SecurityDomainException securityException:
                    statusCode = 403;
                    message = securityException.Message;
                    errors = new { Code = securityException.Code, RiskLevel = securityException.RiskLevel };
                    break;

                case UnauthorizedAccessException:
                    statusCode = 401;
                    message = "دسترسی غیرمجاز";
                    break;
            }

            _logger.LogError(exception, 
                "An error occurred while processing request. Status: {StatusCode}, Message: {Message}", 
                statusCode, message);

            context.Result = new ObjectResult(new
            {
                Status = statusCode,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            })
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }
    }
} 