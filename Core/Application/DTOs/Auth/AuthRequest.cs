using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// درخواست ورود به سیستم
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// نام کاربری یا ایمیل
        /// </summary>
        [Required(ErrorMessage = "نام کاربری یا ایمیل الزامی است")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        public string Password { get; set; }

        /// <summary>
        /// آیا دستگاه را به خاطر بسپار
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        public string DeviceInfo { get; set; }
    }

    /// <summary>
    /// درخواست ثبت‌نام کاربر جدید
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; set; }

        /// <summary>
        /// تأیید رمز عبور
        /// </summary>
        [Required(ErrorMessage = "تأیید رمز عبور الزامی است")]
        [Compare("Password", ErrorMessage = "رمز عبور و تأیید آن مطابقت ندارند")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// درخواست تمدید توکن
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// توکن رفرش
        /// </summary>
        [Required(ErrorMessage = "توکن رفرش الزامی است")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        public string DeviceInfo { get; set; }
    }

    /// <summary>
    /// درخواست تأیید دو مرحله‌ای
    /// </summary>
    public class TwoFactorRequest
    {
        /// <summary>
        /// کد تأیید
        /// </summary>
        [Required(ErrorMessage = "کد تأیید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تأیید باید 6 رقمی باشد")]
        public string Code { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// آیا از کد بازیابی استفاده می‌شود
        /// </summary>
        public bool IsRecoveryCode { get; set; }
    }
} 