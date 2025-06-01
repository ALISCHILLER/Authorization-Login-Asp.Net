using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Configurations
{
    /// <summary>
    /// تنظیمات امنیتی پیشرفته
    /// این کلاس شامل تنظیمات اضافی برای افزایش امنیت سیستم است
    /// </summary>
    public class AdvancedSecuritySettings
    {
        /// <summary>
        /// فعال‌سازی محافظت در برابر حملات XSS
        /// </summary>
        public bool EnableXssProtection { get; set; } = true;

        /// <summary>
        /// فعال‌سازی محافظت در برابر حملات CSRF
        /// </summary>
        public bool EnableCsrfProtection { get; set; } = true;

        /// <summary>
        /// فعال‌سازی محافظت در برابر حملات SQL Injection
        /// </summary>
        public bool EnableSqlInjectionProtection { get; set; } = true;

        /// <summary>
        /// فعال‌سازی محافظت در برابر حملات Brute Force
        /// </summary>
        public bool EnableBruteForceProtection { get; set; } = true;

        /// <summary>
        /// تنظیمات محافظت در برابر حملات Brute Force
        /// </summary>
        public BruteForceProtectionSettings BruteForceProtection { get; set; } = new();

        /// <summary>
        /// فعال‌سازی محافظت در برابر حملات Session Hijacking
        /// </summary>
        public bool EnableSessionHijackingProtection { get; set; } = true;

        /// <summary>
        /// تنظیمات محافظت در برابر حملات Session Hijacking
        /// </summary>
        public SessionProtectionSettings SessionProtection { get; set; } = new();
    }

    /// <summary>
    /// تنظیمات محافظت در برابر حملات Brute Force
    /// </summary>
    public class BruteForceProtectionSettings
    {
        /// <summary>
        /// حداکثر تعداد تلاش مجاز در یک بازه زمانی
        /// </summary>
        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 5;

        /// <summary>
        /// مدت زمان بازه زمانی (به دقیقه)
        /// </summary>
        [Range(1, 60)]
        public int TimeWindowMinutes { get; set; } = 15;

        /// <summary>
        /// مدت زمان قفل شدن حساب (به دقیقه)
        /// </summary>
        [Range(1, 1440)]
        public int LockoutDurationMinutes { get; set; } = 30;

        /// <summary>
        /// آیا باید IP کاربر قفل شود
        /// </summary>
        public bool LockByIp { get; set; } = true;

        /// <summary>
        /// آیا باید نام کاربری قفل شود
        /// </summary>
        public bool LockByUsername { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات محافظت در برابر حملات Session Hijacking
    /// </summary>
    public class SessionProtectionSettings
    {
        /// <summary>
        /// مدت زمان اعتبار نشست (به دقیقه)
        /// </summary>
        [Range(1, 1440)]
        public int SessionTimeoutMinutes { get; set; } = 30;

        /// <summary>
        /// آیا باید نشست با تغییر IP باطل شود
        /// </summary>
        public bool InvalidateOnIpChange { get; set; } = true;

        /// <summary>
        /// آیا باید نشست با تغییر User Agent باطل شود
        /// </summary>
        public bool InvalidateOnUserAgentChange { get; set; } = true;

        /// <summary>
        /// آیا باید نشست با تغییر مکان جغرافیایی باطل شود
        /// </summary>
        public bool InvalidateOnLocationChange { get; set; } = true;

        /// <summary>
        /// حداکثر تعداد نشست‌های همزمان برای هر کاربر
        /// </summary>
        [Range(1, 10)]
        public int MaxConcurrentSessions { get; set; } = 3;
    }
}