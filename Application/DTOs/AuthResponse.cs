using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ احراز هویت شامل اطلاعات توکن و کاربر
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ایمیل کاربر
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام کامل کاربر (در صورت نیاز)
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// نقش فعلی کاربر
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// لیست مجوزهای کاربر (Permissionها)
        /// </summary>
        public List<string> Permissions { get; set; } = new();

        /// <summary>
        /// توکن دسترسی (JWT Token)
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی (برای استفاده سمت کلاینت)
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// توکن رفرش (برای تمدید توکن)
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن رفرش
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
