using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base
{
    /// <summary>
    /// کلاس پایه برای سرویس‌های مدیریت اعلان‌ها
    /// این کلاس شامل متدهای مشترک برای ارسال انواع مختلف اعلان‌ها است
    /// </summary>
    public abstract class BaseNotificationService : BaseService
    {
        protected readonly EmailOptions _emailOptions;
        protected readonly NotificationOptions _notificationOptions;

        protected BaseNotificationService(
            ILogger logger,
            IMemoryCache cache,
            IOptions<EmailOptions> emailOptions,
            IOptions<NotificationOptions> notificationOptions,
            int cacheExpirationMinutes = 30)
            : base(logger, cache, cacheExpirationMinutes)
        {
            _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
            _notificationOptions = notificationOptions?.Value ?? throw new ArgumentNullException(nameof(notificationOptions));
        }

        /// <summary>
        /// ارسال اعلان به کاربر
        /// </summary>
        protected async Task SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            NotificationPriority priority = NotificationPriority.Normal,
            Dictionary<string, string> metadata = null)
        {
            await ExecuteWithLoggingAsync($"ارسال اعلان {type} به کاربر {userId}", async () =>
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Priority = priority,
                    Status = NotificationStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Metadata = metadata ?? new Dictionary<string, string>()
                };

                // ذخیره اعلان در دیتابیس
                await SaveNotificationAsync(notification);

                // ارسال اعلان از طریق کانال‌های مختلف
                await SendNotificationThroughChannelsAsync(notification);

                // به‌روزرسانی وضعیت اعلان
                notification.Status = NotificationStatus.Sent;
                await UpdateNotificationAsync(notification);
            });
        }

        /// <summary>
        /// ارسال اعلان سیستم
        /// </summary>
        protected async Task SendSystemAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            await SendNotificationAsync(
                userId,
                title,
                message,
                NotificationType.System,
                priority,
                new Dictionary<string, string> { { "AlertType", "System" } });
        }

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        protected async Task SendSecurityAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.High)
        {
            await SendNotificationAsync(
                userId,
                title,
                message,
                NotificationType.Security,
                priority,
                new Dictionary<string, string> { { "AlertType", "Security" } });
        }

        /// <summary>
        /// ارسال اعلان عملکرد
        /// </summary>
        protected async Task SendPerformanceAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            await SendNotificationAsync(
                userId,
                title,
                message,
                NotificationType.Performance,
                priority,
                new Dictionary<string, string> { { "AlertType", "Performance" } });
        }

        /// <summary>
        /// ارسال اعلان خطا
        /// </summary>
        protected async Task SendErrorAlertAsync(
            string userId,
            string title,
            string message,
            Exception exception = null,
            NotificationPriority priority = NotificationPriority.High)
        {
            var metadata = new Dictionary<string, string> { { "AlertType", "Error" } };
            if (exception != null)
            {
                metadata["ExceptionType"] = exception.GetType().Name;
                metadata["ExceptionMessage"] = exception.Message;
                metadata["StackTrace"] = exception.StackTrace;
            }

            await SendNotificationAsync(
                userId,
                title,
                message,
                NotificationType.Error,
                priority,
                metadata);
        }

        /// <summary>
        /// ارسال اعلان از طریق کانال‌های مختلف
        /// </summary>
        protected abstract Task SendNotificationThroughChannelsAsync(Notification notification);

        /// <summary>
        /// ذخیره اعلان در دیتابیس
        /// </summary>
        protected abstract Task SaveNotificationAsync(Notification notification);

        /// <summary>
        /// به‌روزرسانی وضعیت اعلان در دیتابیس
        /// </summary>
        protected abstract Task UpdateNotificationAsync(Notification notification);

        /// <summary>
        /// دریافت اعلان‌های کاربر
        /// </summary>
        protected async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            string userId,
            Func<Task<IEnumerable<Notification>>> getNotifications)
        {
            return await ExecuteWithLoggingAsync($"دریافت اعلان‌های کاربر {userId}", async () =>
            {
                var cacheKey = $"UserNotifications_{userId}";
                return await GetOrSetCacheAsync(cacheKey, getNotifications);
            });
        }

        /// <summary>
        /// پاک کردن کش اعلان‌های کاربر
        /// </summary>
        protected void ClearUserNotificationCache(string userId)
        {
            RemoveFromCache($"UserNotifications_{userId}");
        }
    }
} 