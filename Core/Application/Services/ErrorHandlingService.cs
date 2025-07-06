using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Logging; // Added for ILogger
using System; // Added for Exception
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task LogUserErrorAsync(string? userId, string errorMessage, Exception ex)
        {
            // Log with structured logging if possible, including exception details.
            // The specific log level might depend on the severity or type of error.
            // For user-related errors that are handled and presented to the user, Warning might be appropriate.
            // If it's an unexpected server-side issue related to a user operation, Error might be better.
            _logger.LogWarning(ex, "User Error: UserId: {UserId}, Message: {ErrorMessage}", userId ?? "N/A", errorMessage);

            // In a real scenario, you might also send this to a dedicated error tracking service.
            return Task.CompletedTask;
        }

        public Task LogSystemErrorAsync(string systemComponent, string errorMessage, Exception ex)
        {
            // System errors are typically more severe.
            _logger.LogError(ex, "System Error: Component: {SystemComponent}, Message: {ErrorMessage}", systemComponent, errorMessage);

            // In a real scenario, you might also send this to a dedicated error tracking service
            // with higher priority or different tagging.
            return Task.CompletedTask;
        }
    }
}
