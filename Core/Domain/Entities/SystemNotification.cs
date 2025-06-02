using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// کلاس اعلان سیستمی
    /// این کلاس برای ذخیره و مدیریت اعلان‌های سیستمی استفاده می‌شود
    /// </summary>
    public class SystemNotification : BaseEntity
    {
        /// <summary>
        /// عنوان اعلان
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// پیام اعلان
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// شدت اعلان
        /// </summary>
        public AlertSeverity Severity { get; private set; }

        /// <summary>
        /// زمان ایجاد اعلان
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// زمان انقضای اعلان
        /// </summary>
        public DateTime? ExpiresAt { get; private set; }

        /// <summary>
        /// آیا اعلان خوانده شده است
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// زمان خوانده شدن اعلان
        /// </summary>
        public DateTime? ReadAt { get; private set; }

        /// <summary>
        /// شناسه کاربری که اعلان برای او ارسال شده است
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        public NotificationType Type { get; private set; }

        /// <summary>
        /// متادیتای اعلان
        /// </summary>
        public string Metadata { get; private set; }

        private SystemNotification() { }

        /// <summary>
        /// ایجاد یک اعلان سیستمی جدید
        /// </summary>
        public static SystemNotification Create(
            string title,
            string message,
            AlertSeverity severity,
            Guid? userId = null,
            DateTime? expiresAt = null,
            string metadata = null)
        {
            return new SystemNotification
            {
                Title = title,
                Message = message,
                Severity = severity,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsRead = false,
                UserId = userId,
                Type = NotificationType.System,
                Metadata = metadata
            };
        }

        /// <summary>
        /// علامت‌گذاری اعلان به عنوان خوانده شده
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// بررسی انقضای اعلان
        /// </summary>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// به‌روزرسانی متادیتای اعلان
        /// </summary>
        public void UpdateMetadata(string metadata)
        {
            Metadata = metadata;
        }
    }
} 