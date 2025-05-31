using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل پاسخ احراز هویت
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// نام نقش
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// وضعیت تأیید ایمیل
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }

        /// <summary>
        /// لیست دسترسی‌ها
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// زمان انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// زمان انقضای توکن رفرش
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string Token { get; internal set; }
        public UserDto User { get; internal set; }
    }
}
