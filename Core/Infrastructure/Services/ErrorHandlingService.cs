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

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class ErrorHandlingService : Authorization_Login_Asp.Net.Core.Application.Interfaces.IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITracingService _tracingService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly INotificationService _notificationService;
        private readonly IMetricsService _metricsService;
        private readonly ApplicationDbContext _dbContext;
        private string _lastError = string.Empty;
        private string _lastStackTrace = string.Empty;
        private string _lastSource = string.Empty;
        private string _lastMessage = string.Empty;

        public ErrorHandlingService(
            ILogger<ErrorHandlingService> logger,
            IConfiguration configuration,
            ITracingService tracingService,
            IEmailService emailService,
            ISmsService smsService,
            INotificationService notificationService,
            IMetricsService metricsService,
            ApplicationDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _metricsService = metricsService;
            _dbContext = dbContext;
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
                    // پیاده‌سازی ارسال اعلان
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
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.ErrorLogs.AddAsync(errorLog);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging exception");
            }
        }

        public async Task<ErrorResponse> CreateErrorResponseAsync(Exception ex, HttpContext context)
        {
            var response = new ErrorResponse
            {
                Message = "خطای سرور",
                StatusCode = StatusCodes.Status500InternalServerError,
                ErrorTime = DateTime.UtcNow
            };

            switch (ex)
            {
                case DomainException domainEx:
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = domainEx.Message;
                    response.Code = domainEx.Code;
                    response.AdditionalData = domainEx.AdditionalData;
                    break;

                case NotFoundException notFoundEx:
                    response.StatusCode = StatusCodes.Status404NotFound;
                    response.Message = notFoundEx.Message;
                    response.Code = notFoundEx.Code;
                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "EntityType", notFoundEx.EntityType },
                        { "EntityId", notFoundEx.EntityId }
                    };
                    break;

                case ConflictException conflictEx:
                    response.StatusCode = StatusCodes.Status409Conflict;
                    response.Message = conflictEx.Message;
                    response.Code = conflictEx.Code;
                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "EntityType", conflictEx.EntityType },
                        { "ConflictingValue", conflictEx.ConflictingValue }
                    };
                    break;

                case SecurityDomainException securityEx:
                    response.StatusCode = StatusCodes.Status403Forbidden;
                    response.Message = securityEx.Message;
                    response.Code = securityEx.Code;
                    response.AdditionalData = new Dictionary<string, object>
                    {
                        { "RiskLevel", securityEx.RiskLevel },
                        { "IpAddress", securityEx.IpAddress },
                        { "UserAgent", securityEx.UserAgent }
                    };
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "دسترسی غیرمجاز";
                    response.Code = DomainErrorCodes.General.UnauthorizedAccess;
                    break;

                default:
                    _logger.LogError(ex, "An unhandled exception has occurred");
                    break;
            }

            return response;
        }

        private bool IsCriticalError(Exception ex)
        {
            return ex is OutOfMemoryException ||
                   ex is StackOverflowException ||
                   ex is ThreadAbortException ||
                   ex is SecurityException;
        }

        /// <summary>
        /// ثبت خطای سیستمی
        /// </summary>
        public async Task LogSystemErrorAsync(Exception ex, string context, string? userId = null)
        {
            try
            {
                var systemError = new SystemError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace ?? string.Empty,
                    Context = context,
                    UserId = userId ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.SystemErrors.AddAsync(systemError);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error occurred while logging system error");
            }
        }

        /// <summary>
        /// ثبت خطای امنیتی
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
                    CreatedAt = DateTime.UtcNow
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
        /// ثبت خطای اعتبارسنجی
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
                    CreatedAt = DateTime.UtcNow
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
        /// ثبت خطای عملکردی
        /// </summary>
        public async Task LogPerformanceErrorAsync(string message, string context, long duration)
        {
            // پیاده‌سازی ثبت خطای عملکردی
        }

        /// <summary>
        /// دریافت خطاهای سیستمی
        /// </summary>
        public async Task<SystemError[]> GetSystemErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // پیاده‌سازی دریافت خطاهای سیستمی
            return null;
        }

        /// <summary>
        /// دریافت خطاهای امنیتی
        /// </summary>
        public async Task<SecurityError[]> GetSecurityErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // پیاده‌سازی دریافت خطاهای امنیتی
            return null;
        }

        /// <summary>
        /// دریافت خطاهای اعتبارسنجی
        /// </summary>
        public async Task<ValidationError[]> GetValidationErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // پیاده‌سازی دریافت خطاهای اعتبارسنجی
            return null;
        }

        /// <summary>
        /// دریافت خطاهای عملکردی
        /// </summary>
        public async Task<PerformanceError[]> GetPerformanceErrorsAsync(DateTime startDate, DateTime endDate)
        {
            // پیاده‌سازی دریافت خطاهای عملکردی
            return null;
        }

        /// <summary>
        /// پاکسازی خطاهای قدیمی
        /// </summary>
        public async Task CleanupOldErrorsAsync(int daysToKeep)
        {
            // پیاده‌سازی پاکسازی خطاهای قدیمی
        }

        private string GetCurrentIpAddress()
        {
            // TODO: پیاده‌سازی دریافت IP آدرس فعلی
            return "127.0.0.1";
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ErrorTime { get; set; }
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}