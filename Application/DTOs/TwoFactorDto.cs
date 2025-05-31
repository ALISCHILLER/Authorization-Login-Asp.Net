using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل داده‌ای احراز هویت دو مرحله‌ای (با قابلیت به خاطر سپردن)؛ این کلاس برای ارسال کد تأیید (با اعتبارسنجی الزامی بودن و طول دقیق 6 کاراکتر)، شناسه کاربر و وضعیت به خاطر سپردن استفاده می‌شود.
    /// </summary>
    public class TwoFactorDto
    {
        /// <summary>
        /// کد تأیید (با اعتبارسنجی الزامی بودن و طول دقیق 6 کاراکتر)
        /// </summary>
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تأیید باید 6 رقمی باشد")]
        public string Code { get; set; }

        /// <summary>
        /// شناسه کاربر (با اعتبارسنجی الزامی بودن)
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// وضعیت به خاطر سپردن (اختیاری)
        /// </summary>
        public bool RememberMe { get; set; }
    }
} 