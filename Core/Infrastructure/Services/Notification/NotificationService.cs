using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Notification
{
    /// <summary>
    /// سرویس یکپارچه مدیریت اعلان‌ها
    /// این سرویس شامل تمام عملیات مربوط به ارسال و مدیریت اعلان‌ها است
    /// </summary>
    public class NotificationService : BaseNotificationService, INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public NotificationService(
            ILogger<NotificationService> logger,
            IMemoryCache cache,
            IOptions<EmailOptions> emailOptions,
            IOptions<NotificationOptions> notificationOptions,
            AppDbContext context,
            IEmailService emailService)
            : base(logger, cache, emailOptions, notificationOptions)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        }

        /// <summary>
        /// ارسال اعلان به کاربر
        /// </summary>
        public async Task SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            NotificationPriority priority = NotificationPriority.Normal,
            Dictionary<string, string> metadata = null)
        {
            await base.SendNotificationAsync(userId, title, message, type, priority, metadata);
        }

        /// <summary>
        /// ارسال اعلان سیستم
        /// </summary>
        public async Task SendSystemAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            await base.SendSystemAlertAsync(userId, title, message, priority);
        }

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        public async Task SendSecurityAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.High)
        {
            await base.SendSecurityAlertAsync(userId, title, message, priority);
        }

        /// <summary>
        /// ارسال اعلان عملکرد
        /// </summary>
        public async Task SendPerformanceAlertAsync(
            string userId,
            string title,
            string message,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            await base.SendPerformanceAlertAsync(userId, title, message, priority);
        }

        /// <summary>
        /// ارسال اعلان خطا
        /// </summary>
        public async Task SendErrorAlertAsync(
            string userId,
            string title,
            string message,
            Exception exception = null,
            NotificationPriority priority = NotificationPriority.High)
        {
            await base.SendErrorAlertAsync(userId, title, message, exception, priority);
        }

        /// <summary>
        /// دریافت اعلان‌های کاربر
        /// </summary>
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await base.GetUserNotificationsAsync(userId, async () =>
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            });
        }

        /// <summary>
        /// به‌روزرسانی وضعیت اعلان
        /// </summary>
        public async Task UpdateNotificationStatusAsync(string notificationId, NotificationStatus status)
        {
            await ExecuteWithLoggingAsync($"به‌روزرسانی وضعیت اعلان {notificationId}", async () =>
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                {
                    throw new DomainException($"اعلان با شناسه {notificationId} یافت نشد");
                }

                notification.Status = status;
                notification.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                ClearUserNotificationCache(notification.UserId);
            });
        }

        /// <summary>
        /// حذف اعلان
        /// </summary>
        public async Task DeleteNotificationAsync(string notificationId)
        {
            await ExecuteWithLoggingAsync($"حذف اعلان {notificationId}", async () =>
            {
                var notification = await _context.Notifications.FindAsync(notificationId);
                if (notification == null)
                {
                    throw new DomainException($"اعلان با شناسه {notificationId} یافت نشد");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                ClearUserNotificationCache(notification.UserId);
            });
        }

        /// <summary>
        /// ارسال اعلان از طریق کانال‌های مختلف
        /// </summary>
        protected override async Task SendNotificationThroughChannelsAsync(Notification notification)
        {
            var user = await _context.Users.FindAsync(notification.UserId);
            if (user == null) return;

            // ارسال اعلان از طریق ایمیل
            if (_notificationOptions.EnableEmailNotifications && !string.IsNullOrEmpty(user.Email))
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    notification.Title,
                    notification.Message,
                    notification.Type.ToString());
            }

            // ارسال اعلان از طریق پیامک
            if (_notificationOptions.EnableSmsNotifications && !string.IsNullOrEmpty(user.PhoneNumber))
            {
                // TODO: پیاده‌سازی ارسال پیامک
            }

            // ارسال اعلان از طریق اپلیکیشن موبایل
            if (_notificationOptions.EnablePushNotifications && !string.IsNullOrEmpty(user.DeviceToken))
            {
                // TODO: پیاده‌سازی ارسال پوش نوتیفیکیشن
            }
        }

        /// <summary>
        /// ذخیره اعلان در دیتابیس
        /// </summary>
        protected override async Task SaveNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// به‌روزرسانی وضعیت اعلان در دیتابیس
        /// </summary>
        protected override async Task UpdateNotificationAsync(Notification notification)
        {
            notification.UpdatedAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }
    }
} 