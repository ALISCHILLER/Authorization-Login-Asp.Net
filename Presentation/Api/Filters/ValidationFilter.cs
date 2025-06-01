using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Presentation.Api.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
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
                        Message = "Validation failed",
                        Errors = errors,
                        Timestamp = DateTime.UtcNow
                    });
                    return;
                }

                await next();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during validation");
                context.Result = new ObjectResult(new
                {
                    Status = 500,
                    Message = "An error occurred during validation",
                    Timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}