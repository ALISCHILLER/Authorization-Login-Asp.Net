using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    /// <summary>
    /// کلاس پایه برای تمام DTOها
    /// </summary>
    public abstract class BaseDto
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// آیا رکورد حذف شده است؟
        /// </summary>
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// کلاس پایه برای DTOهای پاسخ
    /// </summary>
    public abstract class BaseResponseDto : BaseDto
    {
        /// <summary>
        /// آیا عملیات موفق بوده است؟
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام نتیجه
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// لیست خطاها
        /// </summary>
        public IReadOnlyList<string> Errors { get; set; }
    }

    /// <summary>
    /// کلاس پایه برای DTOهای درخواست
    /// </summary>
    public abstract class BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر درخواست کننده
        /// </summary>
        public Guid? RequestedBy { get; set; }

        /// <summary>
        /// تاریخ درخواست
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
} 