using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// اطلاعات دستگاه
    /// </summary>
    public class DeviceInfoDto
    {
        /// <summary>
        /// شناسه یکتای دستگاه
        /// </summary>
        [Required(ErrorMessage = "شناسه دستگاه الزامی است")]
        [StringLength(100, ErrorMessage = "شناسه دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DeviceId { get; set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        [Required(ErrorMessage = "نام دستگاه الزامی است")]
        [StringLength(100, ErrorMessage = "نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه (موبایل، دسکتاپ، تبلت)
        /// </summary>
        [Required(ErrorMessage = "نوع دستگاه الزامی است")]
        [StringLength(50, ErrorMessage = "نوع دستگاه نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string DeviceType { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        [Required(ErrorMessage = "سیستم عامل الزامی است")]
        [StringLength(100, ErrorMessage = "سیستم عامل نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        [Required(ErrorMessage = "مرورگر الزامی است")]
        [StringLength(100, ErrorMessage = "مرورگر نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Browser { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        [Required(ErrorMessage = "User Agent الزامی است")]
        [StringLength(500, ErrorMessage = "User Agent نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// اطلاعات موقعیت مکانی
    /// </summary>
    public class LocationDto
    {
        /// <summary>
        /// کشور
        /// </summary>
        [StringLength(100, ErrorMessage = "نام کشور نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Country { get; set; }

        /// <summary>
        /// شهر
        /// </summary>
        [StringLength(100, ErrorMessage = "نام شهر نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string City { get; set; }

        /// <summary>
        /// عرض جغرافیایی
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// طول جغرافیایی
        /// </summary>
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// درخواست ورود به سیستم
    /// </summary>
    public class LoginRequestDto : BaseRequestDto
    {
        /// <summary>
        /// نام کاربری یا ایمیل
        /// </summary>
        [Required(ErrorMessage = "نام کاربری یا ایمیل الزامی است")]
        [StringLength(100, ErrorMessage = "نام کاربری یا ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید بین 6 تا 100 کاراکتر باشد")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// آیا مرا به خاطر بسپار
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [Required(ErrorMessage = "آدرس IP الزامی است")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "فرمت آدرس IP نامعتبر است")]
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات موقعیت مکانی
        /// </summary>
        public LocationDto Location { get; set; }
    }

    /// <summary>
    /// پاسخ احراز هویت
    /// </summary>
    public class AuthResponseDto : BaseResponseDto
    {
        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// توکن تجدید
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن تجدید
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        public UserDto User { get; set; }

        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است؟
        /// </summary>
        public bool IsTwoFactorEnabled { get; set; }

        /// <summary>
        /// آیا نیاز به احراز هویت دو مرحله‌ای است؟
        /// </summary>
        public bool RequiresTwoFactor { get; set; }
    }

    /// <summary>
    /// درخواست تجدید توکن
    /// </summary>
    public class RefreshTokenRequestDto : BaseRequestDto
    {
        /// <summary>
        /// توکن تجدید
        /// </summary>
        [Required(ErrorMessage = "توکن تجدید الزامی است")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [Required(ErrorMessage = "آدرس IP الزامی است")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "فرمت آدرس IP نامعتبر است")]
        public string IpAddress { get; set; }
    }

    /// <summary>
    /// درخواست خروج از سیستم
    /// </summary>
    public class LogoutRequestDto : BaseRequestDto
    {
        /// <summary>
        /// توکن دسترسی
        /// </summary>
        [Required(ErrorMessage = "توکن دسترسی الزامی است")]
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن تجدید
        /// </summary>
        [Required(ErrorMessage = "توکن تجدید الزامی است")]
        public string RefreshToken { get; set; }
    }
} 