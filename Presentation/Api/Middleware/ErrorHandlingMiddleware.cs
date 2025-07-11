using Authorization_Login_Asp.Net.Core.Application.Exceptions; // For custom exceptions
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common; // Added for ErrorDetailDto

namespace Authorization_Login_Asp.Net.Presentation.Api.Middleware
{
    // Note: The ApiErrorResponse class definition was previously here and has been moved
    // to Core/Application/DTOs/Common/ErrorDetailDto.cs.

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
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _errorHandlingService = errorHandlingService ?? throw new ArgumentNullException(nameof(errorHandlingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            _logger.LogError(exception, "Unhandled exception occurred. Path: {Path}, User: {UserId}",
                             context.Request.Path, context.User?.FindFirst("sub")?.Value ?? "Anonymous");

            // Call the service to create the error response DTO
            var errorResponse = await _errorHandlingService.CreateErrorDetailDtoAsync(exception, context);
            var userId = context.User?.FindFirst("sub")?.Value;
            string errorMessageToLog = errorResponse.Errors != null ? "Validation failed." : errorResponse.Message;

            if (!string.IsNullOrEmpty(userId))
            {
                await _errorHandlingService.LogUserErrorAsync(userId, errorMessageToLog, exception);
            }
            else
            {
                await _errorHandlingService.LogSystemErrorAsync(context.Request.Path.ToString(), errorMessageToLog, exception);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.StatusCode;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }

        // The GenerateErrorResponse method, previously here, has been removed.
        // Its logic is now centralized in IErrorHandlingService.CreateErrorDetailDtoAsync.
    }
}