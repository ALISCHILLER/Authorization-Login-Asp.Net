using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// درخواست به‌روزرسانی تنظیمات امنیتی کاربر
    /// </summary>
    public class SecuritySettingsRequest
    {
        /// <summary>
        /// فعال بودن اعلان‌های ورود
        /// </summary>
        public bool EnableLoginNotifications { get; set; }

        /// <summary>
        /// فعال بودن اعلان‌های تغییر رمز عبور
        /// </summary>
        public bool EnablePasswordChangeNotifications { get; set; }

        /// <summary>
        /// فعال بودن اعلان‌های تغییر ایمیل
        /// </summary>
        public bool EnableEmailChangeNotifications { get; set; }

        /// <summary>
        /// فعال بودن اعلان‌های تغییر شماره تلفن
        /// </summary>
        public bool EnablePhoneChangeNotifications { get; set; }

        /// <summary>
        /// حداکثر تعداد تلاش‌های ناموفق ورود قبل از قفل شدن حساب
        /// </summary>
        [Range(3, 10, ErrorMessage = "تعداد تلاش‌های ناموفق باید بین ۳ تا ۱۰ باشد.")]
        public int MaxFailedLoginAttempts { get; set; }

        /// <summary>
        /// مدت زمان قفل شدن حساب به دقیقه
        /// </summary>
        [Range(5, 1440, ErrorMessage = "مدت زمان قفل شدن باید بین ۵ تا ۱۴۴۰ دقیقه باشد.")]
        public int AccountLockoutDuration { get; set; }

        /// <summary>
        /// مدت زمان اعتبار رمز عبور به روز
        /// </summary>
        [Range(30, 365, ErrorMessage = "مدت زمان اعتبار رمز عبور باید بین ۳۰ تا ۳۶۵ روز باشد.")]
        public int PasswordExpirationDays { get; set; }
    }
}