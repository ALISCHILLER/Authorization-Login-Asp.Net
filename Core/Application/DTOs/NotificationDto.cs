using System;
using System.ComponentModel.DataAnnotations;
using NotificationType = Authorization_Login_Asp.Net.Core.Domain.Entities.NotificationType;
using AlertSeverity = Authorization_Login_Asp.Net.Core.Domain.Entities.AlertSeverity;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// درخواست ایجاد اعلان جدید
    /// </summary>
    public class NotificationRequest
    {
        /// <summary>
        /// عنوان اعلان
        /// </summary>
        [Required(ErrorMessage = "عنوان اعلان الزامی است")]
        [StringLength(200, ErrorMessage = "عنوان اعلان نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Title { get; set; }

        /// <summary>
        /// متن پیام اعلان
        /// </summary>
        [Required(ErrorMessage = "متن پیام اعلان الزامی است")]
        [StringLength(1000, ErrorMessage = "متن پیام اعلان نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string Message { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        [Required(ErrorMessage = "نوع اعلان الزامی است")]
        public NotificationType Type { get; set; }

        /// <summary>
        /// سطح اهمیت اعلان
        /// </summary>
        [Required(ErrorMessage = "سطح اهمیت اعلان الزامی است")]
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده اعلان (اختیاری)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// تاریخ انقضای اعلان (اختیاری)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// پاسخ اعلان برای نمایش به کاربر
    /// </summary>
    public class NotificationResponse
    {
        /// <summary>
        /// شناسه یکتای اعلان
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// عنوان اعلان
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// متن پیام اعلان
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// سطح اهمیت اعلان
        /// </summary>
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده اعلان
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// وضعیت خوانده شدن اعلان
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// زمان ایجاد اعلان
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// زمان خوانده شدن اعلان
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// تاریخ انقضای اعلان
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// وضعیت منقضی شدن اعلان
        /// </summary>
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    /// <summary>
    /// فیلترهای جستجوی اعلان‌ها
    /// </summary>
    public class NotificationFilter
    {
        /// <summary>
        /// نوع اعلان
        /// </summary>
        public NotificationType? Type { get; set; }

        /// <summary>
        /// سطح اهمیت اعلان
        /// </summary>
        public AlertSeverity? Severity { get; set; }

        /// <summary>
        /// وضعیت خوانده شدن
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// تعداد آیتم‌ها در هر صفحه
        /// </summary>
        [Range(1, 100, ErrorMessage = "تعداد آیتم‌ها در هر صفحه باید بین 1 تا 100 باشد")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// شماره صفحه
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگتر از صفر باشد")]
        public int PageNumber { get; set; } = 1;
    }
}