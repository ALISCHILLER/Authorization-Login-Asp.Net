using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    /// <summary>
    /// نوع اعلان
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// اطلاعات
        /// </summary>
        Info = 0,

        /// <summary>
        /// موفقیت
        /// </summary>
        Success = 1,

        /// <summary>
        /// هشدار
        /// </summary>
        Warning = 2,

        /// <summary>
        /// خطا
        /// </summary>
        Error = 3
    }

    /// <summary>
    /// اولویت اعلان
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// پایین
        /// </summary>
        Low = 0,

        /// <summary>
        /// متوسط
        /// </summary>
        Medium = 1,

        /// <summary>
        /// بالا
        /// </summary>
        High = 2,

        /// <summary>
        /// بحرانی
        /// </summary>
        Critical = 3
    }

    /// <summary>
    /// وضعیت اعلان
    /// </summary>
    public enum NotificationStatus
    {
        /// <summary>
        /// جدید
        /// </summary>
        New = 0,

        /// <summary>
        /// خوانده شده
        /// </summary>
        Read = 1,

        /// <summary>
        /// بایگانی شده
        /// </summary>
        Archived = 2,

        /// <summary>
        /// حذف شده
        /// </summary>
        Deleted = 3
    }

    /// <summary>
    /// اطلاعات اعلان
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// شناسه اعلان
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// شناسه کاربر دریافت‌کننده
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// عنوان اعلان
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// متن اعلان
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// اولویت اعلان
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// آیا اعلان خوانده شده است
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// تاریخ خوانده شدن
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// داده‌های اضافی
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// درخواست ایجاد اعلان جدید
    /// </summary>
    public class CreateNotificationRequest
    {
        /// <summary>
        /// شناسه کاربر دریافت‌کننده
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// عنوان اعلان
        /// </summary>
        [Required(ErrorMessage = "عنوان اعلان الزامی است")]
        [StringLength(100, ErrorMessage = "عنوان اعلان نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Title { get; set; }

        /// <summary>
        /// متن اعلان
        /// </summary>
        [Required(ErrorMessage = "متن اعلان الزامی است")]
        [StringLength(500, ErrorMessage = "متن اعلان نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Message { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        [Required(ErrorMessage = "نوع اعلان الزامی است")]
        public string Type { get; set; }

        /// <summary>
        /// اولویت اعلان
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// تاریخ انقضا
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// داده‌های اضافی
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی وضعیت اعلان
    /// </summary>
    public class UpdateNotificationStatusRequest
    {
        /// <summary>
        /// شناسه اعلان
        /// </summary>
        [Required(ErrorMessage = "شناسه اعلان الزامی است")]
        public string NotificationId { get; set; }

        /// <summary>
        /// آیا اعلان خوانده شده است
        /// </summary>
        public bool IsRead { get; set; }
    }

    /// <summary>
    /// درخواست حذف اعلان
    /// </summary>
    public class DeleteNotificationRequest
    {
        /// <summary>
        /// شناسه اعلان
        /// </summary>
        [Required(ErrorMessage = "شناسه اعلان الزامی است")]
        public string NotificationId { get; set; }
    }

    /// <summary>
    /// درخواست دریافت اعلان‌ها
    /// </summary>
    public class GetNotificationsRequest
    {
        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// وضعیت خوانده شدن
        /// </summary>
        public bool? IsRead { get; set; }

        /// <summary>
        /// نوع اعلان
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// اولویت اعلان
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// پاسخ دریافت اعلان‌ها
    /// </summary>
    public class GetNotificationsResponse
    {
        /// <summary>
        /// لیست اعلان‌ها
        /// </summary>
        public List<NotificationDto> Notifications { get; set; }

        /// <summary>
        /// تعداد کل اعلان‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد اعلان‌های خوانده نشده
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 