using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public interface IErrorHandlingService
    {
        Task HandleExceptionAsync(Exception ex, HttpContext context);
        Task LogErrorAsync(Exception ex, string source, Dictionary<string, object> additionalData = null);
        Task<ErrorResponse> CreateErrorResponseAsync(Exception ex, HttpContext context);
    }

    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IMetricsService _metricsService;
        private readonly IConfiguration _configuration;

        public ErrorHandlingService(
            ILogger<ErrorHandlingService> logger,
            INotificationService notificationService,
            IMetricsService metricsService,
            IConfiguration configuration)
        {
            _logger = logger;
            _notificationService = notificationService;
            _metricsService = metricsService;
            _configuration = configuration;
        }

        public async Task HandleExceptionAsync(Exception ex, HttpContext context)
        {
            try
            {
                var errorResponse = await CreateErrorResponseAsync(ex, context);
                await LogErrorAsync(ex, context.Request.Path, errorResponse.AdditionalData);

                // ارسال اعلان برای خطاهای بحرانی
                if (IsCriticalError(ex))
                {
                    await _notificationService.SendErrorAlertAsync(
                        "Critical Error Occurred",
                        $"Error: {ex.Message}\nPath: {context.Request.Path}\nTime: {DateTime.UtcNow}",
                        AlertSeverity.Critical);
                }

                // ثبت متریک خطا
                _metricsService.IncrementErrorCount(context.Request.Path);

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

        public async Task LogErrorAsync(Exception ex, string source, Dictionary<string, object> additionalData = null)
        {
            try
            {
                var logData = new
                {
                    Timestamp = DateTime.UtcNow,
                    Source = source,
                    ExceptionType = ex.GetType().Name,
                    ex.Message,
                    ex.StackTrace,
                    InnerException = ex.InnerException?.Message,
                    AdditionalData = additionalData
                };

                // لاگ کردن به فایل
                _logger.LogError(ex, "Error occurred in {Source}: {Message}", source, ex.Message);

                // لاگ کردن به Elasticsearch یا سایر سیستم‌های لاگینگ
                if (_configuration.GetValue<bool>("AppSettings:LoggingSettings:EnableElasticsearchLogging"))
                {
                    // پیاده‌سازی ارسال به Elasticsearch
                }

                // لاگ کردن به Application Insights
                if (_configuration.GetValue<bool>("AppSettings:LoggingSettings:EnableApplicationInsights"))
                {
                    // پیاده‌سازی ارسال به Application Insights
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging exception");
            }
        }

        public async Task<ErrorResponse> CreateErrorResponseAsync(Exception ex, HttpContext context)
        {
            var errorResponse = new ErrorResponse
            {
                TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
                Message = GetUserFriendlyMessage(ex),
                StatusCode = GetStatusCode(ex),
                AdditionalData = new Dictionary<string, object>
                {
                    { "Path", context.Request.Path },
                    { "Method", context.Request.Method },
                    { "Timestamp", DateTime.UtcNow }
                }
            };

            // اضافه کردن اطلاعات بیشتر در محیط توسعه
            if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                errorResponse.DeveloperMessage = ex.Message;
                errorResponse.StackTrace = ex.StackTrace;
            }

            return errorResponse;
        }

        private bool IsCriticalError(Exception ex)
        {
            return ex is OutOfMemoryException ||
                   ex is StackOverflowException ||
                   ex is ThreadAbortException ||
                   ex is SecurityException;
        }

        private string GetUserFriendlyMessage(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => "You are not authorized to perform this action.",
                ValidationException => "The request contains invalid data.",
                NotFoundException => "The requested resource was not found.",
                ConflictException => "The request conflicts with the current state of the server.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }

        private int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ValidationException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }

    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public string Message { get; set; }
        public string DeveloperMessage { get; set; }
        public string StackTrace { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}