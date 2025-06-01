using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// مدل پاسخ اطلاعات کاربر (به همراه لیست نقش‌ها)؛ این کلاس برای ارسال اطلاعات کاربر (شامل شناسه، نام کاربری، ایمیل، نام کامل، شماره تلفن، وضعیت فعال بودن، زمان ایجاد، زمان آخرین ورود و لیست نقش‌ها) استفاده می‌شود.
    /// </summary>
    public class UserResponse
    {
        /// <summary>
        /// شناسه یکتا (GUID) کاربر
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری (نام‌ کاربری)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل کاربر (با اعتبارسنجی در لایه‌های دیگر)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام کامل (نام و نام خانوادگی) کاربر
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شماره تلفن (اختیاری)
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// وضعیت فعال بودن کاربر (فعال یا غیرفعال)
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// زمان ایجاد (ثبت‌نام) کاربر
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// زمان آخرین ورود (اختیاری)
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// آدرس عکس پروفایل کاربر
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// نوع نقش کاربر
        /// </summary>
        public RoleType RoleType { get; set; }

        /// <summary>
        /// لیست نام‌های نقش‌های اختصاص داده شده به کاربر (به صورت پیش‌فرض خالی)
        /// </summary>
        public IEnumerable<RoleDto> Roles { get; set; }
    }
}