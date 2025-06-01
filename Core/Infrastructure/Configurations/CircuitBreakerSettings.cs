using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Configurations
{
    /// <summary>
    /// تنظیمات Circuit Breaker برای مدیریت خطاها و جلوگیری از شکست آبشاری در سیستم
    /// این کلاس پارامترهای مورد نیاز برای پیاده‌سازی الگوی Circuit Breaker را تعریف می‌کند
    /// </summary>
    public class CircuitBreakerSettings
    {
        /// <summary>
        /// تعداد خطاهای مجاز قبل از فعال شدن Circuit Breaker
        /// این مقدار مشخص می‌کند که چند خطا باید رخ دهد تا مدار قطع شود
        /// </summary>
        [Range(1, 10, ErrorMessage = "تعداد خطاهای مجاز باید بین 1 تا 10 باشد")]
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 2;

        /// <summary>
        /// مدت زمان (به ثانیه) که Circuit Breaker در حالت باز (قطع) باقی می‌ماند
        /// در این مدت، درخواست‌ها مستقیماً رد می‌شوند تا سرویس فرصت بازیابی داشته باشد
        /// </summary>
        [Range(1, 300, ErrorMessage = "مدت زمان قطع مدار باید بین 1 تا 300 ثانیه باشد")]
        public int DurationOfBreak { get; set; } = 30;

        /// <summary>
        /// تعداد دفعات تلاش مجدد قبل از قطع کامل مدار
        /// این مقدار مشخص می‌کند که چند بار باید قبل از قطع کامل مدار، تلاش مجدد انجام شود
        /// </summary>
        [Range(1, 10, ErrorMessage = "تعداد تلاش‌های مجدد باید بین 1 تا 10 باشد")]
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// فاصله زمانی (به ثانیه) بین هر تلاش مجدد
        /// این مقدار مشخص می‌کند که چه مدت باید بین هر تلاش مجدد صبر کرد
        /// </summary>
        [Range(1, 60, ErrorMessage = "فاصله زمانی بین تلاش‌ها باید بین 1 تا 60 ثانیه باشد")]
        public int RetryInterval { get; set; } = 2;

        /// <summary>
        /// فعال یا غیرفعال کردن قابلیت Circuit Breaker
        /// در صورت غیرفعال بودن، تمام درخواست‌ها مستقیماً به سرویس ارسال می‌شوند
        /// </summary>
        public bool EnableCircuitBreaker { get; set; } = true;
    }
}