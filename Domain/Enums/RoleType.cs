using System;

namespace Authorization_Login_Asp.Net.Domain.Enums
{
    /// <summary>
    /// انواع نقش کاربران در سیستم
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// کاربر عادی
        /// </summary>
        User = 0,

        /// <summary>
        /// مدیر سیستم
        /// </summary>
        Admin = 1,

        /// <summary>
        /// اپراتور
        /// </summary>
        Operator = 2,

        /// <summary>
        /// مدیر محتوا
        /// </summary>
        ContentManager = 3,

        /// <summary>
        /// پشتیبان
        /// </summary>
        Support = 4
    }
} 