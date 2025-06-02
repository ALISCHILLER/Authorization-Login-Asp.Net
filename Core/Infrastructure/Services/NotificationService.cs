using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// سرویس مدیریت اعلان‌های سیستم
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// ارسال اعلان سیستمی
        /// </summary>
        Task SendSystemAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// ارسال اعلان عملکردی
        /// </summary>
        Task SendPerformanceAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// ارسال اعلان خطا
        /// </summary>
        Task SendErrorAlertAsync(string title, string message, AlertSeverity severity);

        /// <summary>
        /// دریافت لیست اعلان‌ها
        /// </summary>
        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10);

        /// <summary>
        /// دریافت لیست اعلان‌ها با فیلتر
        /// </summary>
        Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(NotificationFilter filter);

        /// <summary>
        /// ایجاد اعلان جدید
        /// </summary>
        Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request);

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان خوانده شده
        /// </summary>
        Task MarkAsReadAsync(Guid id);

        /// <summary>
        /// حذف اعلان
        /// </summary>
        Task DeleteNotificationAsync(Guid id);

        /// <summary>
        /// پاکسازی اعلان‌های منقضی شده
        /// </summary>
        Task CleanupExpiredNotificationsAsync();
    }

    /// <summary>
    /// سرویس یکپارچه اعلان‌ها
    /// این سرویس تمام عملیات مربوط به ارسال اعلان‌ها (ایمیل، پیامک، اعلان‌های سیستمی) را مدیریت می‌کند
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;
        private readonly IConfiguration _configuration;
        private readonly ITracingService _tracingService;
        private readonly NotificationOptions _notificationOptions;

        public NotificationService(
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
            IConfiguration configuration,
            ITracingService tracingService,
            IOptions<NotificationOptions> notificationOptions)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _notificationOptions = notificationOptions?.Value ?? throw new ArgumentNullException(nameof(notificationOptions));
        }

        #region Email Notifications

        public async Task SendVerificationEmailAsync(string email, Guid userId)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendVerificationEmailAsync");
            try
            {
                var verificationUrl = $"{_configuration["Application:BaseUrl"]}/api/auth/verify-email?userId={userId}";
                var subject = "تأیید ایمیل";
                var message = $@"
                    سلام،
                    برای تأیید ایمیل خود، لطفاً روی لینک زیر کلیک کنید:
                    {verificationUrl}
                    
                    این لینک تا 24 ساعت معتبر است.
                    در صورتی که این درخواست از سوی شما نبوده است، لطفاً این ایمیل را نادیده بگیرید.
                ";

                await SendEmailAsync(email, subject, message, NotificationPriority.High);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل تأیید به {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendPasswordResetEmailAsync");
            try
            {
                var resetUrl = $"{_configuration["Application:BaseUrl"]}/api/auth/reset-password?token={resetToken}";
                var subject = "بازنشانی رمز عبور";
                var message = $@"
                    سلام،
                    برای بازنشانی رمز عبور خود، لطفاً روی لینک زیر کلیک کنید:
                    {resetUrl}
                    
                    این لینک تا 1 ساعت معتبر است.
                    در صورتی که این درخواست از سوی شما نبوده است، لطفاً این ایمیل را نادیده بگیرید.
                ";

                await SendEmailAsync(email, subject, message, NotificationPriority.High);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل بازنشانی رمز عبور به {Email}", email);
                throw;
            }
        }

        public async Task SendTwoFactorCodeAsync(string recipient, string code, NotificationType type)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendTwoFactorCodeAsync");
            try
            {
                var subject = "کد تأیید دو مرحله‌ای";
                var message = $"کد تأیید شما: {code}\nاین کد تا 5 دقیقه معتبر است.";

                await SendNotificationAsync(recipient, subject, message, type, NotificationPriority.High);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال کد تأیید دو مرحله‌ای به {Recipient}", recipient);
                throw;
            }
        }

        public async Task SendBackupCodesAsync(string email, List<string> backupCodes)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendBackupCodesAsync");
            try
            {
                var subject = "کدهای پشتیبان احراز هویت دو مرحله‌ای";
                var message = $@"
                    سلام،
                    کدهای پشتیبان احراز هویت دو مرحله‌ای شما به شرح زیر است:
                    
                    {string.Join("\n", backupCodes)}
                    
                    لطفاً این کدها را در جای امنی نگهداری کنید.
                    هر کد فقط یک بار قابل استفاده است.
                ";

                await SendEmailAsync(email, subject, message, NotificationPriority.High);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال کدهای پشتیبان به {Email}", email);
                throw;
            }
        }

        public async Task SendSecurityAlertAsync(string email, string alertType, string details)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendSecurityAlertAsync");
            try
            {
                var subject = $"هشدار امنیتی: {alertType}";
                var message = $@"
                    سلام،
                    یک فعالیت مشکوک در حساب کاربری شما شناسایی شده است:
                    
                    نوع هشدار: {alertType}
                    جزئیات: {details}
                    
                    در صورتی که این فعالیت از سوی شما نبوده است، لطفاً فوراً با پشتیبانی تماس بگیرید.
                ";

                await SendEmailAsync(email, subject, message, NotificationPriority.Critical);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال هشدار امنیتی به {Email}", email);
                throw;
            }
        }

        private async Task SendEmailAsync(
            string email,
            string subject,
            string message,
            NotificationPriority priority,
            Dictionary<string, string> metadata = null)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendEmailAsync");
            try
            {
                var emailRequest = new EmailRequest
                {
                    To = email,
                    Subject = subject,
                    Body = message,
                    Priority = priority,
                    Metadata = metadata
                };

                await _emailService.SendEmailAsync(emailRequest);
                _logger.LogInformation("ایمیل با موفقیت به {Email} ارسال شد", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل به {Email}", email);
                throw;
            }
        }

        #endregion

        #region SMS Notifications

        public async Task SendVerificationSmsAsync(string phoneNumber, string code)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendVerificationSmsAsync");
            try
            {
                await _smsService.SendVerificationCodeAsync(phoneNumber, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال پیامک تأیید");
                throw;
            }
        }

        public async Task SendTwoFactorCodeSmsAsync(string phoneNumber, string code)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendTwoFactorCodeSmsAsync");
            try
            {
                await _smsService.SendTwoFactorCodeAsync(phoneNumber, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال پیامک کد دو مرحله‌ای");
                throw;
            }
        }

        #endregion

        #region System Notifications

        public async Task SendSystemAlertAsync(string title, string message, AlertSeverity severity)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendSystemAlertAsync");
            try
            {
                // ثبت اعلان در دیتابیس
                var notification = new SystemNotification
                {
                    Title = title,
                    Message = message,
                    Severity = severity,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveSystemNotificationAsync(notification);

                // ارسال به کانال‌های مختلف بر اساس شدت اعلان
                switch (severity)
                {
                    case AlertSeverity.Critical:
                        await SendCriticalAlertAsync(notification);
                        break;
                    case AlertSeverity.Error:
                        await SendErrorAlertAsync(notification);
                        break;
                    case AlertSeverity.Warning:
                        await SendWarningAlertAsync(notification);
                        break;
                    default:
                        await SendInfoAlertAsync(notification);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان سیستمی");
                throw;
            }
        }

        public async Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendSecurityAlertAsync");
            try
            {
                var notification = new SecurityNotification
                {
                    Title = title,
                    Message = message,
                    Severity = severity,
                    CreatedAt = DateTime.UtcNow
                };

                await SaveSecurityNotificationAsync(notification);

                // ارسال به کانال‌های امنیتی
                await SendSecurityAlertToChannelsAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان امنیتی");
                throw;
            }
        }

        #endregion

        #region Private Methods

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SaveVerificationCodeAsync(Guid userId, string code, DateTime expiryTime, NotificationType type)
        {
            // پیاده‌سازی ذخیره کد تأیید در دیتابیس
            throw new NotImplementedException();
        }

        private async Task SaveSystemNotificationAsync(SystemNotification notification)
        {
            // پیاده‌سازی ذخیره اعلان سیستمی در دیتابیس
            throw new NotImplementedException();
        }

        private async Task SaveSecurityNotificationAsync(SecurityNotification notification)
        {
            // پیاده‌سازی ذخیره اعلان امنیتی در دیتابیس
            throw new NotImplementedException();
        }

        private async Task SendCriticalAlertAsync(SystemNotification notification)
        {
            // پیاده‌سازی ارسال اعلان بحرانی به تمام کانال‌ها
            throw new NotImplementedException();
        }

        private async Task SendErrorAlertAsync(SystemNotification notification)
        {
            // پیاده‌سازی ارسال اعلان خطا
            throw new NotImplementedException();
        }

        private async Task SendWarningAlertAsync(SystemNotification notification)
        {
            // پیاده‌سازی ارسال اعلان هشدار
            throw new NotImplementedException();
        }

        private async Task SendInfoAlertAsync(SystemNotification notification)
        {
            // پیاده‌سازی ارسال اعلان اطلاعاتی
            throw new NotImplementedException();
        }

        private async Task SendSecurityAlertToChannelsAsync(SecurityNotification notification)
        {
            // پیاده‌سازی ارسال اعلان امنیتی به کانال‌های مربوطه
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// ارسال اعلان عملکردی
        /// </summary>
        public async Task SendPerformanceAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = Notification.Create(
                    title: title,
                    message: message,
                    type: NotificationType.Performance,
                    severity: severity,
                    userId: null,
                    expiryDate: null);

                await _notificationRepository.AddAsync(notification);
                _logger.LogWarning("اعلان عملکردی ارسال شد: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان عملکردی");
                throw;
            }
        }

        /// <summary>
        /// دریافت لیست اعلان‌ها
        /// </summary>
        public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int count = 10)
        {
            var filter = new NotificationFilter
            {
                PageSize = count,
                PageNumber = 1
            };
            return await GetNotificationsAsync(filter);
        }

        /// <summary>
        /// دریافت لیست اعلان‌ها با فیلتر
        /// </summary>
        public async Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(NotificationFilter filter)
        {
            try
            {
                var notifications = await _notificationRepository.GetNotificationsAsync(filter);
                return notifications.Select(MapToResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست اعلان‌ها");
                throw;
            }
        }

        /// <summary>
        /// ایجاد اعلان جدید
        /// </summary>
        public async Task<NotificationResponse> CreateNotificationAsync(NotificationRequest request)
        {
            try
            {
                var notification = Notification.Create(
                    title: request.Title,
                    message: request.Message,
                    type: request.Type,
                    severity: request.Severity,
                    userId: request.UserId,
                    expiryDate: request.ExpiryDate);

                var createdNotification = await _notificationRepository.AddAsync(notification);
                return MapToResponse(createdNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد اعلان جدید");
                throw;
            }
        }

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان خوانده شده
        /// </summary>
        public async Task MarkAsReadAsync(Guid id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"اعلان با شناسه {id} یافت نشد");
                }

                notification.MarkAsRead();
                await _notificationRepository.UpdateAsync(notification);
                _logger.LogInformation("اعلان با شناسه {Id} به عنوان خوانده شده علامت‌گذاری شد", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در علامت‌گذاری اعلان به عنوان خوانده شده");
                throw;
            }
        }

        /// <summary>
        /// حذف اعلان
        /// </summary>
        public async Task DeleteNotificationAsync(Guid id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"اعلان با شناسه {id} یافت نشد");
                }

                await _notificationRepository.DeleteAsync(notification);
                _logger.LogInformation("اعلان با شناسه {Id} حذف شد", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف اعلان");
                throw;
            }
        }

        /// <summary>
        /// پاکسازی اعلان‌های منقضی شده
        /// </summary>
        public async Task CleanupExpiredNotificationsAsync()
        {
            try
            {
                var expiredNotifications = await _notificationRepository.GetExpiredNotificationsAsync();
                foreach (var notification in expiredNotifications)
                {
                    await _notificationRepository.DeleteAsync(notification);
                }
                _logger.LogInformation("{Count} اعلان منقضی شده پاکسازی شد", expiredNotifications.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی اعلان‌های منقضی شده");
                throw;
            }
        }

        /// <summary>
        /// تبدیل موجودیت اعلان به مدل پاسخ
        /// </summary>
        private static NotificationResponse MapToResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Severity = notification.Severity,
                UserId = notification.UserId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                ExpiryDate = notification.ExpiryDate
            };
        }

        public async Task SendNotificationAsync(
            string recipient,
            string subject,
            string message,
            NotificationType type,
            NotificationPriority priority = NotificationPriority.Normal,
            Dictionary<string, string> metadata = null)
        {
            using var activity = _tracingService.StartActivity("NotificationService.SendNotificationAsync");
            try
            {
                switch (type)
                {
                    case NotificationType.Email:
                        await SendEmailAsync(recipient, subject, message, priority, metadata);
                        break;

                    case NotificationType.Sms:
                        await SendSmsAsync(recipient, message, priority, metadata);
                        break;

                    case NotificationType.Both:
                        await Task.WhenAll(
                            SendEmailAsync(recipient, subject, message, priority, metadata),
                            SendSmsAsync(recipient, message, priority, metadata));
                        break;

                    default:
                        throw new DomainException("نوع اعلان نامعتبر است");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان به {Recipient} از نوع {Type}", recipient, type);
                throw;
            }
        }
    }
}