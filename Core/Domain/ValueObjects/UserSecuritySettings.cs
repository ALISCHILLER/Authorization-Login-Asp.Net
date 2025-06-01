using System;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// تنظیمات امنیتی کاربر به عنوان یک Value Object
    /// این کلاس شامل تمام تنظیمات مربوط به امنیت حساب کاربری است
    /// </summary>
    public class UserSecuritySettings
    {
        /// <summary>
        /// نیاز به تغییر رمز عبور در ورود بعدی
        /// </summary>
        public bool RequirePasswordChange { get; set; }

        /// <summary>
        /// تاریخ انقضای رمز عبور
        /// </summary>
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// حداکثر تعداد تلاش‌های ناموفق برای ورود
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// مدت زمان قفل شدن حساب پس از تلاش‌های ناموفق (دقیقه)
        /// </summary>
        public int AccountLockoutDurationMinutes { get; set; } = 30;

        /// <summary>
        /// نیاز به رمز عبور قوی
        /// </summary>
        public bool RequireStrongPassword { get; set; } = true;

        /// <summary>
        /// تعداد رمزهای عبور قبلی که نباید تکرار شوند
        /// </summary>
        public int PasswordHistoryCount { get; set; } = 5;

        /// <summary>
        /// نیاز به رمزهای عبور یکتا
        /// </summary>
        public bool RequireUniquePasswords { get; set; } = true;

        /// <summary>
        /// مدت زمان منقضی شدن نشست (دقیقه)
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// نیاز به تأیید آدرس IP
        /// </summary>
        public bool RequireIpValidation { get; set; }

        /// <summary>
        /// نیاز به تأیید دستگاه
        /// </summary>
        public bool RequireDeviceValidation { get; set; }

        /// <summary>
        /// نیاز به تأیید موقعیت مکانی
        /// </summary>
        public bool RequireLocationValidation { get; set; }

        /// <summary>
        /// نیاز به تأیید ایمیل
        /// </summary>
        public bool RequireEmailVerification { get; set; } = true;

        /// <summary>
        /// نیاز به تأیید شماره تلفن
        /// </summary>
        public bool RequirePhoneVerification { get; set; }

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای دستگاه‌های جدید
        /// </summary>
        public bool RequireTwoFactorForNewDevices { get; set; }

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای عملیات‌های حساس
        /// </summary>
        public bool RequireTwoFactorForSensitiveOperations { get; set; }

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای دسترسی ادمین
        /// </summary>
        public bool RequireTwoFactorForAdminAccess { get; set; } = true;

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای دسترسی از خارج
        /// </summary>
        public bool RequireTwoFactorForExternalAccess { get; set; }

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای تراکنش‌های با ارزش بالا
        /// </summary>
        public bool RequireTwoFactorForHighValueTransactions { get; set; }

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای تغییر رمز عبور
        /// </summary>
        public bool RequireTwoFactorForPasswordChange { get; set; } = true;

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای تغییر ایمیل
        /// </summary>
        public bool RequireTwoFactorForEmailChange { get; set; } = true;

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای تغییر شماره تلفن
        /// </summary>
        public bool RequireTwoFactorForPhoneChange { get; set; } = true;

        /// <summary>
        /// نیاز به احراز هویت دو مرحله‌ای برای تغییر تنظیمات امنیتی
        /// </summary>
        public bool RequireTwoFactorForSecuritySettingsChange { get; set; } = true;
    }
}