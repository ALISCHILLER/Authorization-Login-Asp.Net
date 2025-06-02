using System;

namespace Authorization_Login_Asp.Net.Core.Application.Exceptions
{
    /// <summary>
    /// استثنا برای مواردی که حساب کاربری قفل شده است
    /// این استثنا زمانی رخ می‌دهد که کاربر به دلیل تلاش‌های ناموفق متعدد یا دلایل امنیتی دیگر قفل شده است
    /// </summary>
    public class AccountLockedException : Exception
    {
        /// <summary>
        /// سازنده پیش‌فرض با پیام خطای پیش‌فرض
        /// </summary>
        public AccountLockedException() : base("حساب کاربری قفل شده است")
        {
        }

        /// <summary>
        /// سازنده با پیام خطای سفارشی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        public AccountLockedException(string message) : base(message)
        {
        }

        /// <summary>
        /// سازنده با پیام خطا و استثنای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        public AccountLockedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// زمان پایان قفل شدن حساب
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// تعداد دقیقه‌های باقی‌مانده تا پایان قفل شدن
        /// </summary>
        public int RemainingMinutes { get; set; }
    }
}