using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.Exceptions;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
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

                context.Result = new BadRequestObjectResult(new
                {
                    error = "خطا در اعتبارسنجی داده‌ها",
                    errors = errors
                });
                return;
            }

            await next();
        }
    }

    public class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );

                context.Result = new BadRequestObjectResult(new
                {
                    error = "خطا در اعتبارسنجی داده‌ها",
                    errors = errors
                });
                context.ExceptionHandled = true;
            }
            else if (context.Exception is DomainException domainException)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    error = domainException.Message
                });
                context.ExceptionHandled = true;
            }
        }
    }
} 