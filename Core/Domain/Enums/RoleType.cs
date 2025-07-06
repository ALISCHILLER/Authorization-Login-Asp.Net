using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// انواع نقش‌های کاربری در سیستم
    /// هر مقدار یک نوع خاص از دسترسی یا وضعیت کاربر را مشخص می‌کند.
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// کاربر تأیید نشده - ایمیل کاربر هنوز تأیید نشده است
        /// </summary>
        Unverified = 0,

        /// <summary>
        /// مدیر ارشد - دسترسی کامل به تمام بخش‌ها
        /// </summary>
        SuperAdmin = 1,

        /// <summary>
        /// مدیر - دسترسی محدود به بخش‌های مدیریتی
        /// </summary>
        Admin = 2,

        /// <summary>
        /// نقش موقت - دسترسی محدود زمانی
        /// </summary>
        Temporary = 3,

        /// <summary>
        /// کاربر عادی - دسترسی پایه به سیستم
        /// </summary>
        User = 4,

        /// <summary>
        /// کاربر مهمان - دسترسی محدود و بدون اجازه تغییرات
        /// </summary>
        Guest = 5,

        /// <summary>
        /// اپراتور - کاربر با دسترسی عملیاتی محدود به سیستم
        /// </summary>
        Operator = 6,

        /// <summary>
        /// مدیر محتوا - دسترسی به مدیریت محتوا و اطلاعات عمومی
        /// </summary>
        ContentManager = 7,

        /// <summary>
        /// پشتیبانی - کاربر با دسترسی به بخش پشتیبانی و خدمات کاربری
        /// </summary>
        Support = 8,

        /// <summary>
        /// نقش محدود شده - دسترسی بسیار محدود به منابع خاص
        /// </summary>
        Restricted = 9,

        /// <summary>
        /// نقش سیستمی - برای عملیات‌های خودکار سیستم
        /// </summary>
        System = 10
    }
}