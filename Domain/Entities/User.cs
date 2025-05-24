using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Domain.Enums;
using Authorization_Login_Asp.Net.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل کاربر سیستم
    /// نماینده جدول Users در دیتابیس
    /// </summary>
    public class User
    {
        /// <summary>
        /// کلید اصلی (Primary Key)
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری یکتا برای ورود به سیستم
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل کاربر به صورت Value Object (مقدار پیچیده)
        /// برای ذخیره‌سازی باید در کانفیگ EF Core به عنوان Owned Entity تعریف شود
        /// </summary>
        [Required]
        public Email Email { get; set; }

        /// <summary>
        /// هش شده رمز عبور برای نگهداری امن در دیتابیس
        /// هیچ‌گاه رمز عبور به صورت متن ساده ذخیره نمی‌شود
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// نام کامل کاربر (نمایشی در پنل یا گزارش‌ها)
        /// </summary>
        [MaxLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// نقش کاربر در سیستم (Admin, User, Manager و غیره)
        /// استفاده از Enum برای خوانایی بهتر و جلوگیری از اشتباهات تایپی
        /// </summary>
        [Required]
        public UserRole Role { get; set; }

        /// <summary>
        /// تاریخ ایجاد حساب کاربری (ثبت‌نام)
        /// ذخیره به صورت UTC برای یکپارچگی زمانی
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعال بودن حساب کاربری
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// لیست توکن‌های رفرش متعلق به این کاربر
        /// یک کاربر می‌تواند چندین رفرش توکن داشته باشد (مثلاً از چند دستگاه)
        /// مقداردهی اولیه در سازنده برای جلوگیری از خطاهای NullReferenceException
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// سازنده بدون پارامتر برای EF Core
        /// </summary>
        public User()
        {
            RefreshTokens = new List<RefreshToken>();
        }
    }
}
