using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// درخواست ثبت‌نام کاربر جدید
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// نام کاربری (باید منحصر به فرد و معتبر باشد)
        /// </summary>
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل کاربر (باید منحصر به فرد و معتبر باشد)
        /// </summary>
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور کاربر (با الزامات امنیتی مثل حداقل طول و پیچیدگی)
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; set; }

        /// <summary>
        /// تکرار رمز عبور برای اطمینان از مطابقت
        /// </summary>
        [Required(ErrorMessage = "تکرار رمز عبور الزامی است")]
        [Compare("Password", ErrorMessage = "رمز عبور و تکرار آن مطابقت ندارند")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// نام کامل کاربر (اختیاری ولی توصیه‌شده)
        /// </summary>
        [Required(ErrorMessage = "نام کامل الزامی است")]
        [StringLength(100, ErrorMessage = "نام کامل نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string FullName { get; set; }

        /// <summary>
        /// شماره تلفن کاربر (اختیاری)
        /// </summary>
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// نقش کاربر هنگام ثبت‌نام (مثلاً "User") - فقط توسط Admin مقداردهی شود
        /// </summary>
        [RegularExpression("User|Admin|Manager", ErrorMessage = "نقش باید یکی از مقادیر User، Admin یا Manager باشد.")]
        public string Role { get; set; } = "User";
    }
}
