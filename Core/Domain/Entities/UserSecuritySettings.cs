using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettings
    {
        /// <summary>
        /// نمک رمز عبور
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// تعداد تلاش‌های مجاز برای ورود
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// مدت زمان قفل شدن حساب (دقیقه)
        /// </summary>
        public int AccountLockoutDurationMinutes { get; set; } = 30;

        /// <summary>
        /// تاریخ انقضای رمز عبور
        /// </summary>
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// نیاز به تغییر رمز عبور
        /// </summary>
        public bool RequirePasswordChange { get; set; }

        /// <summary>
        /// فعال بودن اعلان ورود جدید
        /// </summary>
        public bool NotifyOnNewLogin { get; set; } = true;

        /// <summary>
        /// فعال بودن اعلان تغییر رمز عبور
        /// </summary>
        public bool NotifyOnPasswordChange { get; set; } = true;

        /// <summary>
        /// فعال بودن اعلان تغییر ایمیل
        /// </summary>
        public bool NotifyOnEmailChange { get; set; } = true;

        /// <summary>
        /// فعال بودن اعلان تغییر شماره تلفن
        /// </summary>
        public bool NotifyOnPhoneChange { get; set; } = true;

        /// <summary>
        /// فعال بودن اعلان فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public bool NotifyOnTwoFactorActivation { get; set; } = true;

        /// <summary>
        /// فعال بودن اعلان غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public bool NotifyOnTwoFactorDeactivation { get; set; } = true;
    }
}