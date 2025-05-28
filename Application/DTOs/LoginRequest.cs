using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// درخواست ورود به سیستم (Login)
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// ایمیل کاربر
        /// </summary>
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        public string Password { get; set; }

        /// <summary>
        /// آیا کاربر را به خاطر بسپار
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        [Required(ErrorMessage = "آدرس IP الزامی است")]
        [MaxLength(50, ErrorMessage = "آدرس IP نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات مرورگر کاربر
        /// </summary>
        [Required(ErrorMessage = "اطلاعات مرورگر الزامی است")]
        [MaxLength(500, ErrorMessage = "اطلاعات مرورگر نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string UserAgent { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی (در صورت دسترسی)
        /// </summary>
        [MaxLength(200, ErrorMessage = "موقعیت جغرافیایی نمی‌تواند بیشتر از 200 کاراکتر باشد")]
        public string Location { get; set; }
    }
}
