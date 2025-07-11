using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Exceptions;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common; // Added for ErrorDetailDto
using Authorization_Login_Asp.Net.Core.Application.Exceptions; // Added for custom exceptions

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    // Renamed class to LoggingAndErrorHandlingService and implemented ILoggingService
    public class LoggingAndErrorHandlingService : Authorization_Login_Asp.Net.Core.Application.Interfaces.IErrorHandlingService, Authorization_Login_Asp.Net.Core.Application.Interfaces.ILoggingService
    {
        private readonly ILogger<LoggingAndErrorHandlingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITracingService _tracingService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly INotificationService _notificationService;
        private readonly IMetricsService _metricsService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService; // Added
        private string _lastError = string.Empty;
        private string _lastStackTrace = string.Empty;
        private string _lastSource = string.Empty;
        private string _lastMessage = string.Empty;

        public LoggingAndErrorHandlingService( // Constructor name updated to match class name
            ILogger<LoggingAndErrorHandlingService> logger,
            IConfiguration configuration,
            ITracingService tracingService,
            IEmailService emailService,
            ISmsService smsService,
            INotificationService notificationService,
            IMetricsService metricsService,
            ApplicationDbContext dbContext,
            IDateTimeService dateTimeService) // Added
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _metricsService = metricsService;
            _dbContext = dbContext;
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService)); // Added
        }

        // This method is not directly used by the new ErrorHandlingMiddleware,
        // as the middleware handles the HttpContext response itself.
        // It can be kept for other internal uses or removed if not needed.
        // For now, commenting out to avoid confusion.
        /*
        public async Task HandleExceptionAsync(Exception ex, HttpContext context)
        {
            try
            {
                var errorResponse = await CreateErrorDetailDtoAsync(ex, context);
                await LogErrorAsync(ex, context.Request.Path.ToString(), errorResponse.AdditionalData);

                // TODO: Implement critical error notification (e.g., email admin)
                if (IsCriticalError(ex))
                {
                    // Send notification for critical errors
                }

                // Record error metric
                _metricsService.IncrementErrorCount(context.Request.Path.ToString());

                // تنظیم پاسخ HTTP
                context.Response.StatusCode = errorResponse.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
            }
            catch (Exception handlingEx)
            {
                _logger.LogError(handlingEx, "Error occurred while handling exception");
                throw;
            }
        }
        */

        // Interface method implementation
        public async Task LogUserErrorAsync(string? userId, string errorMessage, Exception ex)
        {
            _logger.LogWarning(ex, "User Error: UserId: {UserId}, Message: {ErrorMessage}", userId ?? "N/A", errorMessage);
            // Log to general error log
            var errorDetail = await CreateErrorDetailDtoAsync(ex);
            await LogErrorAsync(ex, $"UserActivity_UserId:{userId}", errorDetail.AdditionalData);

            // TODO: Consider if specific user-facing error table needs to be populated
            // or if general ErrorLog is sufficient.
        }

        // Interface method implementation
        public async Task LogSystemErrorAsync(string systemComponent, string errorMessage, Exception ex)
        {
            _logger.LogError(ex, "System Error: Component: {SystemComponent}, Message: {ErrorMessage}", systemComponent, errorMessage);

            // Log to general error log
            var errorDetail = await CreateErrorDetailDtoAsync(ex);
            await LogErrorAsync(ex, $"SystemComponent:{systemComponent}", errorDetail.AdditionalData);

            // Also log to the specific SystemErrors table if needed
            // Note: The existing LogSystemErrorAsync has a different signature, this new one aligns with the interface.
            // We can call the old one internally if desired.
            await LogSystemErrorToTableAsync(ex, systemComponent, null); // Pass null for userId or determine appropriately
        }

        // Existing method for logging to SystemErrors table, renamed to avoid signature clash
        private async Task LogSystemErrorToTableAsync(Exception ex, string context, string? userId = null)
        {
            try
            {
                var systemError = new SystemError // Assuming SystemError entity exists
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    Context = context,
                    UserId = userId ?? string.Empty,
                    CreatedAt = _dateTimeService.UtcNow // Changed
                };

                await _dbContext.SystemErrors.AddAsync(systemError);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging system error to table");
            }
        }


        public async Task LogErrorAsync(Exception ex, string source, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                _lastError = ex.Message;
                _lastStackTrace = ex.StackTrace ?? string.Empty;
                _lastSource = source;
                _lastMessage = ex.Message;

                var errorLog = new ErrorLog
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = source,
                    AdditionalData = additionalData,
                    CreatedAt = _dateTimeService.UtcNow // Changed
                };

                await _dbContext.ErrorLogs.AddAsync(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging exception");
            }
        }

        public Task<ErrorDetailDto> CreateErrorDetailDtoAsync(Exception exception, HttpContext? httpContext = null)
        {
            // This method centralizes the logic for creating a standardized error response DTO.
            var response = new ErrorDetailDto
            {
                Message = "An unexpected error occurred. Please try again later.", // Default user-friendly message
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorTime = _dateTimeService.UtcNow
            };

            switch (exception)
            {
                case ValidationException validationEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Validation failed."; // General validation error message
                    response.Errors = validationEx.Errors;
                    break;
                case BadRequestException badRequestEx: // Custom application BadRequestException
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = badRequestEx.Message;
                    break;
                case Core.Application.Exceptions.NotFoundException notFoundEx: // Custom application NotFoundException
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = notFoundEx.Message;
                    // Example: response.AdditionalData = new Dictionary<string, object> { { "EntityType", notFoundEx.EntityType }, { "EntityId", notFoundEx.EntityId } };
                    break;
                case AccountLockedException accLockEx: // Custom application AccountLockedException
                     response.StatusCode = StatusCodes.Status403Forbidden;
                     response.Message = accLockEx.Message;
                     break;
                case Core.Application.Exceptions.UnauthorizedException unauthorizedEx: // Custom application UnauthorizedException
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = unauthorizedEx.Message;
                    break;
                case Core.Application.Exceptions.ForbiddenException forbiddenEx: // Custom application ForbiddenException
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = forbiddenEx.Message;
                    break;
                case ArgumentException argEx: // Standard .NET ArgumentException (includes ArgumentNullException, ArgumentOutOfRangeException)
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Invalid request: " + argEx.Message; // Or a more generic message
                    break;
                case InvalidOperationException opEx: // Standard .NET InvalidOperationException
                    response.StatusCode = StatusCodes.Status409Conflict; // 409 Conflict is often suitable
                    response.Message = "Operation cannot be performed in the current state: " + opEx.Message;
                    break;
                case System.UnauthorizedAccessException: // Standard .NET UnauthorizedAccessException (typically for authorization issues)
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = "You are not authorized to access this resource.";
                    break;
                // Domain specific exceptions from Core.Domain.Exceptions
                case DomainException domainEx: // General domain exception
                    response.StatusCode = StatusCodes.Status400BadRequest; // Or could be other codes based on domainEx.Code
                    response.Message = domainEx.Message;
                    response.Code = domainEx.Code;
                    response.AdditionalData = domainEx.AdditionalData;
                    break;
                case ConflictException conflictEx: // More specific domain exception (e.g., duplicate entry)
                    response.StatusCode = StatusCodes.Status409Conflict;
                    response.Message = conflictEx.Message;
                    response.Code = conflictEx.Code;
                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "EntityType", conflictEx.EntityType },
                        { "ConflictingValue", conflictEx.ConflictingValue }
                    };
                    break;
                case SecurityDomainException securityEx: // Specific domain exception for security issues
                    response.StatusCode = StatusCodes.Status403Forbidden; // Or potentially 400/401 depending on context
                    response.Message = securityEx.Message;
                    response.Code = securityEx.Code;
                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "RiskLevel", securityEx.RiskLevel },
                        { "IpAddress", securityEx.IpAddress },
                        { "UserAgent", securityEx.UserAgent }
                    };
                    break;
                default:
                    // For any other unhandled exception, the default 500 error message is used.
                    // The actual exception details are logged for developers by the calling middleware.
                    _logger.LogError(exception, "An unhandled exception type ({ExceptionType}) was processed by CreateErrorDetailDtoAsync.", exception.GetType().FullName);
                    // The generic message "An unexpected error occurred..." remains.
                    break;
            }
            return Task.FromResult(response);
        }

        private bool IsCriticalError(Exception ex)
        {
            return ex is OutOfMemoryException ||
                   ex is StackOverflowException ||
                   ex is ThreadAbortException ||
                   ex is SecurityException;
        }

        /// <summary>
        /// Logs a system error to a dedicated table. This is the older version of LogSystemErrorAsync.
        /// </summary>
        private async Task LogSystemErrorToTableAsync(Exception ex, string context, string? userId = null) // Summary added
        {
            try
            {
                var systemError = new SystemError // Assuming SystemError entity exists
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    Context = context,
                    UserId = userId ?? string.Empty,
                    CreatedAt = _dateTimeService.UtcNow // Changed
                };

                await _dbContext.SystemErrors.AddAsync(systemError);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging system error to table");
            }
        }

        /// <summary>
        /// Logs a security-related error.
        /// </summary>
        public async Task LogSecurityErrorAsync(string message, string context, string? userId = null)
        {
            try
            {
                var securityError = new SecurityError
                {
                    Message = message,
                    Context = context,
                    UserId = userId ?? string.Empty,
                    IpAddress = GetCurrentIpAddress(),
                    CreatedAt = _dateTimeService.UtcNow // Changed
                };

                await _dbContext.SecurityErrors.AddAsync(securityError);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging security error");
            }
        }

        /// <summary>
        /// Logs a validation error.
        /// </summary>
        public async Task LogValidationErrorAsync(string message, string context, string? userId = null)
        {
            try
            {
                var validationError = new ValidationError
                {
                    Message = message,
                    Context = context,
                    UserId = userId ?? string.Empty,
                    CreatedAt = _dateTimeService.UtcNow // Changed
                };

                await _dbContext.ValidationErrors.AddAsync(validationError);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging validation error");
            }
        }

        /// <summary>
        /// Logs a performance-related error or issue.
        /// </summary>
        public Task LogPerformanceErrorAsync(string message, string context, long duration)
        {
            // TODO: Implement performance error logging.
            _logger.LogWarning("LogPerformanceErrorAsync is not implemented.");
            throw new NotImplementedException("LogPerformanceErrorAsync is not implemented.");
        }

        /// <summary>
        /// Retrieves system errors within a specified date range.
        /// </summary>
        public Task<SystemError[]> GetSystemErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement retrieval of system errors.
            _logger.LogWarning("GetSystemErrorsAsync is not implemented.");
            throw new NotImplementedException("GetSystemErrorsAsync is not implemented.");
        }

        /// <summary>
        /// Retrieves security errors within a specified date range.
        /// </summary>
        public Task<SecurityError[]> GetSecurityErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement retrieval of security errors.
            _logger.LogWarning("GetSecurityErrorsAsync is not implemented.");
            throw new NotImplementedException("GetSecurityErrorsAsync is not implemented.");
        }

        /// <summary>
        /// Retrieves validation errors within a specified date range.
        /// </summary>
        public Task<ValidationError[]> GetValidationErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement retrieval of validation errors.
            _logger.LogWarning("GetValidationErrorsAsync is not implemented.");
            throw new NotImplementedException("GetValidationErrorsAsync is not implemented.");
        }

        /// <summary>
        /// Retrieves performance errors within a specified date range.
        /// </summary>
        public Task<PerformanceError[]> GetPerformanceErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // TODO: Implement retrieval of performance errors.
            _logger.LogWarning("GetPerformanceErrorsAsync is not implemented.");
            throw new NotImplementedException("GetPerformanceErrorsAsync is not implemented.");
        }

        /// <summary>
        /// Cleans up old error logs older than the specified number of days.
        /// </summary>
        public Task CleanupOldErrorsAsync(int daysToKeep)
        {
            // TODO: Implement cleanup of old error logs.
            _logger.LogWarning("CleanupOldErrorsAsync is not implemented.");
            throw new NotImplementedException("CleanupOldErrorsAsync is not implemented.");
        }

        private string GetCurrentIpAddress()
        {
            // TODO: Implement retrieval of the current IP address. This typically requires IHttpContextAccessor.
            _logger.LogWarning("GetCurrentIpAddress is returning a placeholder and needs proper implementation.");
            // For now, returning a placeholder. In a real scenario, inject IHttpContextAccessor.
            // return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "IP_Not_Available";
            return "127.0.0.1"; // Placeholder
        }

        // Implementation for ILoggingService
        public void LogError(string message)
        {
            _logger.LogError(message);
        }
    }

    // ErrorResponse class is now removed as ErrorDetailDto from Common DTOs will be used.
    // public class ErrorResponse
    // {
    //     public string Message { get; set; } = string.Empty;
    //     public int StatusCode { get; set; }
    //     public string Code { get; set; } = string.Empty;
    //     public DateTime ErrorTime { get; set; }
    //     public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    // }
}