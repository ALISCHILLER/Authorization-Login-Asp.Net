using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// پاسخ احراز هویت
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// آیا عملیات موفق بوده است
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام پاسخ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiration { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن رفرش
        /// </summary>
        public DateTime RefreshTokenExpiration { get; set; }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        public UserInfo UserInfo { get; set; }

        /// <summary>
        /// آیا نیاز به تأیید دو مرحله‌ای است
        /// </summary>
        public bool RequiresTwoFactor { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public string TwoFactorType { get; set; }

        /// <summary>
        /// لیست خطاها
        /// </summary>
        public List<string> Errors { get; set; }
    }

    /// <summary>
    /// اطلاعات کاربر
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string Id { get; set; }

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
        /// آیا ایمیل تأیید شده است
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// آیا شماره تلفن تأیید شده است
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// دسترسی‌های کاربر
        /// </summary>
        public List<string> Permissions { get; set; }
    }

    /// <summary>
    /// پاسخ تنظیمات دو مرحله‌ای
    /// </summary>
    public class TwoFactorSetupResponse
    {
        /// <summary>
        /// آیا عملیات موفق بوده است
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام پاسخ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// کلید مخفی
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// کد QR
        /// </summary>
        public string QrCodeUrl { get; set; }

        /// <summary>
        /// کدهای بازیابی
        /// </summary>
        public List<string> RecoveryCodes { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public string TwoFactorType { get; set; }
    }
} 