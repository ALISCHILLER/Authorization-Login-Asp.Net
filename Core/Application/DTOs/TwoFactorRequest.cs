using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// مدل درخواست تأیید کد احراز هویت دو مرحله‌ای؛ این کلاس برای ارسال کد تأیید (با اعتبارسنجی الزامی بودن و طول دقیق 6 کاراکتر) و شناسه کاربر استفاده می‌شود.
    /// </summary>
    public class TwoFactorRequest
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
    }
}