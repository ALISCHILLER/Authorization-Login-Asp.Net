using System;

namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// انواع نقش‌های کاربری
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// کاربر عادی
        /// </summary>
        User = 4,

        /// <summary>
        /// مدیر
        /// </summary>
        Admin = 2,

        /// <summary>
        /// مدیر ارشد
        /// </summary>
        SuperAdmin = 1,

        /// <summary>
        /// کاربر مهمان
        /// </summary>
        Guest = 5,

        /// <summary>
        /// کاربر محدود شده
        /// </summary>
        Restricted = 3,

        /// <summary>
        /// کاربر تأیید نشده
        /// </summary>
        Unverified = 0
    }
} 