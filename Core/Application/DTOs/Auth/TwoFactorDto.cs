using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// درخواست فعال‌سازی احراز هویت دو مرحله‌ای
    /// </summary>
    public class EnableTwoFactorRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        [Required(ErrorMessage = "نوع احراز هویت دو مرحله‌ای الزامی است")]
        public TwoFactorType Type { get; set; }

        /// <summary>
        /// شماره موبایل (در صورت انتخاب نوع پیامک)
        /// </summary>
        [Phone(ErrorMessage = "فرمت شماره موبایل نامعتبر است")]
        [StringLength(20, ErrorMessage = "شماره موبایل نمی‌تواند بیشتر از 20 کاراکتر باشد")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل (در صورت انتخاب نوع ایمیل)
        /// </summary>
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [StringLength(100, ErrorMessage = "ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Email { get; set; }
    }

    /// <summary>
    /// درخواست غیرفعال‌سازی احراز هویت دو مرحله‌ای
    /// </summary>
    public class DisableTwoFactorRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// کد تایید
        /// </summary>
        [Required(ErrorMessage = "کد تایید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تایید باید 6 رقمی باشد")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "کد تایید فقط می‌تواند شامل اعداد باشد")]
        public string Code { get; set; }
    }

    /// <summary>
    /// درخواست تایید کد احراز هویت دو مرحله‌ای
    /// </summary>
    public class VerifyTwoFactorRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// کد تایید
        /// </summary>
        [Required(ErrorMessage = "کد تایید الزامی است")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "کد تایید باید 6 رقمی باشد")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "کد تایید فقط می‌تواند شامل اعداد باشد")]
        public string Code { get; set; }

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
    }

    /// <summary>
    /// پاسخ تنظیمات احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorSetupResponseDto : BaseResponseDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است؟
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? Type { get; set; }

        /// <summary>
        /// کلید QR (در صورت انتخاب نوع اپلیکیشن)
        /// </summary>
        public string QrCodeKey { get; set; }

        /// <summary>
        /// آدرس QR (در صورت انتخاب نوع اپلیکیشن)
        /// </summary>
        public string QrCodeUri { get; set; }

        /// <summary>
        /// شماره موبایل (در صورت انتخاب نوع پیامک)
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// ایمیل (در صورت انتخاب نوع ایمیل)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// کدهای پشتیبان
        /// </summary>
        public IReadOnlyList<string> RecoveryCodes { get; set; }
    }

    /// <summary>
    /// درخواست تولید کدهای پشتیبان جدید
    /// </summary>
    public class GenerateRecoveryCodesRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
    }

    /// <summary>
    /// پاسخ تولید کدهای پشتیبان جدید
    /// </summary>
    public class GenerateRecoveryCodesResponseDto : BaseResponseDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// کدهای پشتیبان جدید
        /// </summary>
        public IReadOnlyList<string> RecoveryCodes { get; set; }
    }

    /// <summary>
    /// درخواست استفاده از کد پشتیبان
    /// </summary>
    public class UseRecoveryCodeRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// کد پشتیبان
        /// </summary>
        [Required(ErrorMessage = "کد پشتیبان الزامی است")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "کد پشتیبان باید 10 کاراکتری باشد")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "کد پشتیبان فقط می‌تواند شامل حروف انگلیسی بزرگ و اعداد باشد")]
        public string RecoveryCode { get; set; }

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
    }

    /// <summary>
    /// نوع احراز هویت دو مرحله‌ای
    /// </summary>
    public enum TwoFactorType
    {
        /// <summary>
        /// اپلیکیشن احراز هویت
        /// </summary>
        Authenticator = 1,

        /// <summary>
        /// پیامک
        /// </summary>
        Sms = 2,

        /// <summary>
        /// ایمیل
        /// </summary>
        Email = 3
    }
} 