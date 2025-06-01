using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

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
    /// پیاده‌سازی سرویس مدیریت اعلان‌های سیستم
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        /// <summary>
        /// ارسال اعلان سیستمی
        /// </summary>
        public async Task SendSystemAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = Notification.Create(
                    title: title,
                    message: message,
                    type: NotificationType.System,
                    severity: severity,
                    userId: null,
                    expiryDate: null);

                await _notificationRepository.AddAsync(notification);
                _logger.LogInformation("اعلان سیستمی ارسال شد: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان سیستمی");
                throw;
            }
        }

        /// <summary>
        /// ارسال اعلان امنیتی
        /// </summary>
        public async Task SendSecurityAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = Notification.Create(
                    title: title,
                    message: message,
                    type: NotificationType.Security,
                    severity: severity,
                    userId: null,
                    expiryDate: null);

                await _notificationRepository.AddAsync(notification);
                _logger.LogWarning("اعلان امنیتی ارسال شد: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان امنیتی");
                throw;
            }
        }

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
        /// ارسال اعلان خطا
        /// </summary>
        public async Task SendErrorAlertAsync(string title, string message, AlertSeverity severity)
        {
            try
            {
                var notification = Notification.Create(
                    title: title,
                    message: message,
                    type: NotificationType.Error,
                    severity: severity,
                    userId: null,
                    expiryDate: null);

                await _notificationRepository.AddAsync(notification);
                _logger.LogError("اعلان خطا ارسال شد: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اعلان خطا");
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
    }
}