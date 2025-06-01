using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// موجودیت اعلان‌های سیستم
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// شناسه یکتای اعلان
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// عنوان اعلان
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// متن پیام اعلان
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// نوع اعلان (سیستمی، امنیتی، عملکردی، خطا)
        /// </summary>
        public NotificationType Type { get; private set; }

        /// <summary>
        /// سطح اهمیت اعلان (اطلاعات، هشدار، خطا، بحرانی)
        /// </summary>
        public AlertSeverity Severity { get; private set; }

        /// <summary>
        /// زمان ایجاد اعلان
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// وضعیت خوانده شدن اعلان
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده اعلان (در صورت خالی بودن، اعلان عمومی است)
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// تاریخ انقضای اعلان
        /// </summary>
        public DateTime? ExpiryDate { get; private set; }

        /// <summary>
        /// زمان خوانده شدن اعلان
        /// </summary>
        public DateTime? ReadAt { get; private set; }

        // سازنده خصوصی برای EF Core
        private Notification() { }

        /// <summary>
        /// ایجاد یک اعلان جدید
        /// </summary>
        public static Notification Create(
            string title,
            string message,
            NotificationType type,
            AlertSeverity severity,
            Guid? userId = null,
            DateTime? expiryDate = null)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                Severity = severity,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = userId,
                ExpiryDate = expiryDate
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
        /// بررسی منقضی شدن اعلان
        /// </summary>
        public bool IsExpired()
        {
            return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات اعلان
        /// </summary>
        public void Update(string title, string message, AlertSeverity severity)
        {
            Title = title;
            Message = message;
            Severity = severity;
        }
    }

    /// <summary>
    /// انواع اعلان‌های سیستم
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// اعلان‌های سیستمی
        /// </summary>
        System = 1,

        /// <summary>
        /// اعلان‌های امنیتی
        /// </summary>
        Security = 2,

        /// <summary>
        /// اعلان‌های مربوط به عملکرد
        /// </summary>
        Performance = 3,

        /// <summary>
        /// اعلان‌های خطا
        /// </summary>
        Error = 4
    }

    /// <summary>
    /// سطوح اهمیت اعلان‌ها
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// سطح اطلاعاتی
        /// </summary>
        Info = 1,

        /// <summary>
        /// سطح هشدار
        /// </summary>
        Warning = 2,

        /// <summary>
        /// سطح خطا
        /// </summary>
        Error = 3,

        /// <summary>
        /// سطح بحرانی
        /// </summary>
        Critical = 4
    }
}