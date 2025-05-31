using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Domain.Enums;
using Authorization_Login_Asp.Net.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل پایه کاربر
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// نام نقش
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// نقش کاربر
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// وضعیت تأیید ایمیل
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }
    }

    /// <summary>
    /// مدل پاسخ کاربر
    /// </summary>
    public class UserResponseDto : UserDto
    {
        /// <summary>
        /// تاریخ آخرین تغییر رمز عبور
        /// </summary>
        public DateTime? LastPasswordChange { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ناموفق ورود
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// زمان پایان قفل شدن حساب
        /// </summary>
        public DateTime? AccountLockoutEnd { get; set; }

        /// <summary>
        /// آیا حساب قفل شده است
        /// </summary>
        public bool IsLocked => AccountLockoutEnd.HasValue && AccountLockoutEnd.Value > DateTime.UtcNow;

        /// <summary>
        /// آیا نیاز به تغییر رمز عبور است
        /// </summary>
        public bool RequiresPasswordChange { get; set; }

        /// <summary>
        /// تاریخ انقضای رمز عبور
        /// </summary>
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// تعداد دستگاه‌های فعال
        /// </summary>
        public int ActiveDevicesCount { get; set; }

        /// <summary>
        /// تعداد کدهای بازیابی فعال
        /// </summary>
        public int ActiveRecoveryCodesCount { get; set; }
    }

    /// <summary>
    /// مدل ایجاد کاربر
    /// </summary>
    public class CreateUserDto
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool EnableTwoFactor { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }
    }

    /// <summary>
    /// مدل به‌روزرسانی کاربر
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شناسه نقش
        /// </summary>
        public Guid? RoleId { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool? EnableTwoFactor { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }
    }

    /// <summary>
    /// مدل تغییر رمز عبور
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// رمز عبور فعلی
        /// </summary>
        public string CurrentPassword { get; set; }

        /// <summary>
        /// رمز عبور جدید
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// تکرار رمز عبور جدید
        /// </summary>
        public string ConfirmNewPassword { get; set; }
    }

    /// <summary>
    /// مدل تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsDto
    {
        /// <summary>
        /// تعداد حداکثر تلاش‌های ناموفق ورود
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; }

        /// <summary>
        /// مدت زمان قفل شدن حساب (به دقیقه)
        /// </summary>
        public int AccountLockoutDurationMinutes { get; set; }

        /// <summary>
        /// نیاز به تغییر رمز عبور
        /// </summary>
        public bool RequirePasswordChange { get; set; }

        /// <summary>
        /// تاریخ انقضای رمز عبور
        /// </summary>
        public DateTime? PasswordExpiryDate { get; set; }

        /// <summary>
        /// مدت زمان اعتبار توکن رفرش (به روز)
        /// </summary>
        public int RefreshTokenExpiryDays { get; set; }

        /// <summary>
        /// تعداد کدهای بازیابی
        /// </summary>
        public int RecoveryCodesCount { get; set; }
    }
}
