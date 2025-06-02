using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    /// <summary>
    /// کلاس درخواست تغییر رمز عبور
    /// </summary>
    public class ChangePasswordRequestDto : BaseRequestDto
    {
        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        [Required(ErrorMessage = "رمز عبور فعلی الزامی است")]
        public string CurrentPassword { get; set; }

        /// <summary>
        /// رمز عبور جدید
        /// </summary>
        [Required(ErrorMessage = "رمز عبور جدید الزامی است")]
        [MinLength(8, ErrorMessage = "رمز عبور باید حداقل 8 کاراکتر باشد")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "رمز عبور باید شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد")]
        public string NewPassword { get; set; }

        /// <summary>
        /// تکرار رمز عبور جدید
        /// </summary>
        [Required(ErrorMessage = "تکرار رمز عبور جدید الزامی است")]
        [Compare("NewPassword", ErrorMessage = "رمز عبور جدید و تکرار آن مطابقت ندارند")]
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>
    /// کلاس درخواست تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsDto : BaseRequestDto
    {
        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public bool EnableTwoFactor { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }

        /// <summary>
        /// فعال‌سازی ورود با ایمیل
        /// </summary>
        public bool EnableEmailLogin { get; set; }

        /// <summary>
        /// فعال‌سازی ورود با شماره تلفن
        /// </summary>
        public bool EnablePhoneLogin { get; set; }

        /// <summary>
        /// فعال‌سازی ورود با نام کاربری
        /// </summary>
        public bool EnableUsernameLogin { get; set; }

        /// <summary>
        /// فعال‌سازی یادآوری رمز عبور
        /// </summary>
        public bool EnablePasswordReminder { get; set; }

        /// <summary>
        /// تعداد روزهای اعتبار رمز عبور
        /// </summary>
        [Range(30, 365, ErrorMessage = "تعداد روزهای اعتبار رمز عبور باید بین 30 تا 365 روز باشد")]
        public int? PasswordExpirationDays { get; set; }

        /// <summary>
        /// تعداد رمزهای عبور قبلی که قابل استفاده مجدد نیستند
        /// </summary>
        [Range(1, 10, ErrorMessage = "تعداد رمزهای عبور قبلی باید بین 1 تا 10 باشد")]
        public int? PasswordHistoryCount { get; set; }

        /// <summary>
        /// فعال‌سازی محدودیت IP
        /// </summary>
        public bool EnableIpRestriction { get; set; }

        /// <summary>
        /// لیست IP های مجاز
        /// </summary>
        public List<string> AllowedIpAddresses { get; set; }

        /// <summary>
        /// فعال‌سازی محدودیت مرورگر
        /// </summary>
        public bool EnableBrowserRestriction { get; set; }

        /// <summary>
        /// لیست مرورگرهای مجاز
        /// </summary>
        public List<string> AllowedBrowsers { get; set; }

        /// <summary>
        /// فعال‌سازی اعلان‌های امنیتی
        /// </summary>
        public bool EnableSecurityNotifications { get; set; }

        /// <summary>
        /// ارسال اعلان برای ورود از دستگاه جدید
        /// </summary>
        public bool NotifyOnNewDeviceLogin { get; set; }

        /// <summary>
        /// ارسال اعلان برای تغییر رمز عبور
        /// </summary>
        public bool NotifyOnPasswordChange { get; set; }

        /// <summary>
        /// ارسال اعلان برای تغییر ایمیل
        /// </summary>
        public bool NotifyOnEmailChange { get; set; }

        /// <summary>
        /// ارسال اعلان برای تغییر شماره تلفن
        /// </summary>
        public bool NotifyOnPhoneChange { get; set; }

        /// <summary>
        /// ارسال اعلان برای ورود ناموفق
        /// </summary>
        public bool NotifyOnFailedLogin { get; set; }

        /// <summary>
        /// تعداد دفعات مجاز ورود ناموفق
        /// </summary>
        [Range(1, 10, ErrorMessage = "تعداد دفعات مجاز ورود ناموفق باید بین 1 تا 10 باشد")]
        public int? MaxFailedLoginAttempts { get; set; }

        /// <summary>
        /// مدت زمان قفل شدن حساب (دقیقه)
        /// </summary>
        [Range(5, 1440, ErrorMessage = "مدت زمان قفل شدن حساب باید بین 5 تا 1440 دقیقه باشد")]
        public int? AccountLockoutDuration { get; set; }
    }

    /// <summary>
    /// کلاس پاسخ تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsResponseDto : BaseResponseDto
    {
        /// <summary>
        /// وضعیت احراز هویت دو مرحله‌ای
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }

        /// <summary>
        /// تاریخ فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        public DateTime? TwoFactorEnabledAt { get; set; }

        /// <summary>
        /// تاریخ آخرین تغییر رمز عبور
        /// </summary>
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// تاریخ انقضای رمز عبور
        /// </summary>
        public DateTime? PasswordExpiresAt { get; set; }

        /// <summary>
        /// تعداد روزهای باقی‌مانده تا انقضای رمز عبور
        /// </summary>
        public int? DaysUntilPasswordExpires { get; set; }

        /// <summary>
        /// تعداد ورودهای ناموفق
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// تاریخ انقضای قفل حساب
        /// </summary>
        public DateTime? AccountLockoutEndsAt { get; set; }

        /// <summary>
        /// لیست دستگاه‌های فعال
        /// </summary>
        public List<DeviceInfo> ActiveDevices { get; set; }

        /// <summary>
        /// لیست نشست‌های فعال
        /// </summary>
        public List<SessionInfo> ActiveSessions { get; set; }
    }

    /// <summary>
    /// کلاس اطلاعات دستگاه
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// تاریخ آخرین فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// آیا دستگاه فعلی است
        /// </summary>
        public bool IsCurrentDevice { get; set; }
    }

    /// <summary>
    /// کلاس اطلاعات نشست
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// شناسه نشست
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// تاریخ شروع نشست
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// مدت زمان نشست
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// آیا نشست فعلی است
        /// </summary>
        public bool IsCurrentSession { get; set; }
    }
} 