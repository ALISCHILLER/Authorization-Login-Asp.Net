using Authorization_Login_Asp.Net.Core.Domain.Entities;
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

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public interface IErrorHandlingService
    {
        Task HandleExceptionAsync(Exception ex, HttpContext context);
        Task LogErrorAsync(Exception ex, string source, Dictionary<string, object> additionalData = null);
        Task<ErrorResponse> CreateErrorResponseAsync(Exception ex, HttpContext context);
        Task LogSystemErrorAsync(Exception ex, string context, string userId = null);
        Task LogSecurityErrorAsync(string message, string context, string userId = null);
        Task LogValidationErrorAsync(string message, string context, string userId = null);
        Task LogPerformanceErrorAsync(string message, string context, long duration);
        Task<SystemError[]> GetSystemErrorsAsync(DateTime startDate, DateTime endDate);
        Task<SecurityError[]> GetSecurityErrorsAsync(DateTime startDate, DateTime endDate);
        Task<ValidationError[]> GetValidationErrorsAsync(DateTime startDate, DateTime endDate);
        Task<PerformanceError[]> GetPerformanceErrorsAsync(DateTime startDate, DateTime endDate);
        Task CleanupOldErrorsAsync(int daysToKeep);
    }

    /// <summary>
    /// سرویس مدیریت خطا و لاگینگ
    /// </summary>
    public class ErrorHandlingService : BaseService, IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IMetricsService _metricsService;
        private readonly IConfiguration _configuration;

        public ErrorHandlingService(
            IUnitOfWork unitOfWork,
            ILogger<ErrorHandlingService> logger,
            INotificationService notificationService,
            IMetricsService metricsService,
            IConfiguration configuration)
            : base(unitOfWork, logger)
        {
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

        /// <summary>
        /// ثبت خطای سیستمی
        /// </summary>
        public async Task LogSystemErrorAsync(Exception ex, string context, string userId = null)
        {
            await ExecuteInTransaction(async () =>
            {
                var error = new SystemError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source,
                    Context = context,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    ErrorType = ex.GetType().Name
                };

                await _unitOfWork.SystemErrors.AddAsync(error);
                _logger.LogError(ex, $"خطای سیستمی در {context} - کاربر: {userId}");
            }, "ثبت خطای سیستمی");
        }

        /// <summary>
        /// ثبت خطای امنیتی
        /// </summary>
        public async Task LogSecurityErrorAsync(string message, string context, string userId = null)
        {
            await ExecuteInTransaction(async () =>
            {
                var error = new SecurityError
                {
                    Message = message,
                    Context = context,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = GetCurrentIpAddress()
                };

                await _unitOfWork.SecurityErrors.AddAsync(error);
                _logger.LogWarning($"خطای امنیتی در {context} - کاربر: {userId} - پیام: {message}");
            }, "ثبت خطای امنیتی");
        }

        /// <summary>
        /// ثبت خطای اعتبارسنجی
        /// </summary>
        public async Task LogValidationErrorAsync(string message, string context, string userId = null)
        {
            await ExecuteInTransaction(async () =>
            {
                var error = new ValidationError
                {
                    Message = message,
                    Context = context,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.ValidationErrors.AddAsync(error);
                _logger.LogWarning($"خطای اعتبارسنجی در {context} - کاربر: {userId} - پیام: {message}");
            }, "ثبت خطای اعتبارسنجی");
        }

        /// <summary>
        /// ثبت خطای عملکردی
        /// </summary>
        public async Task LogPerformanceErrorAsync(string message, string context, long duration)
        {
            await ExecuteInTransaction(async () =>
            {
                var error = new PerformanceError
                {
                    Message = message,
                    Context = context,
                    Duration = duration,
                    Timestamp = DateTime.UtcNow
                };

                await _unitOfWork.PerformanceErrors.AddAsync(error);
                _logger.LogWarning($"خطای عملکردی در {context} - مدت زمان: {duration}ms - پیام: {message}");
            }, "ثبت خطای عملکردی");
        }

        /// <summary>
        /// دریافت خطاهای سیستمی
        /// </summary>
        public async Task<SystemError[]> GetSystemErrorsAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.SystemErrors.GetErrorsAsync(startDate, endDate);
        }

        /// <summary>
        /// دریافت خطاهای امنیتی
        /// </summary>
        public async Task<SecurityError[]> GetSecurityErrorsAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.SecurityErrors.GetErrorsAsync(startDate, endDate);
        }

        /// <summary>
        /// دریافت خطاهای اعتبارسنجی
        /// </summary>
        public async Task<ValidationError[]> GetValidationErrorsAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.ValidationErrors.GetErrorsAsync(startDate, endDate);
        }

        /// <summary>
        /// دریافت خطاهای عملکردی
        /// </summary>
        public async Task<PerformanceError[]> GetPerformanceErrorsAsync(DateTime startDate, DateTime endDate)
        {
            return await _unitOfWork.PerformanceErrors.GetErrorsAsync(startDate, endDate);
        }

        /// <summary>
        /// پاکسازی خطاهای قدیمی
        /// </summary>
        public async Task CleanupOldErrorsAsync(int daysToKeep)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            
            await ExecuteInTransaction(async () =>
            {
                await _unitOfWork.SystemErrors.DeleteOldErrorsAsync(cutoffDate);
                await _unitOfWork.SecurityErrors.DeleteOldErrorsAsync(cutoffDate);
                await _unitOfWork.ValidationErrors.DeleteOldErrorsAsync(cutoffDate);
                await _unitOfWork.PerformanceErrors.DeleteOldErrorsAsync(cutoffDate);
                
                _logger.LogInformation($"خطاهای قدیمی‌تر از {cutoffDate} پاکسازی شدند");
            }, "پاکسازی خطاهای قدیمی");
        }

        private string GetCurrentIpAddress()
        {
            // TODO: پیاده‌سازی دریافت IP آدرس فعلی
            return "127.0.0.1";
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