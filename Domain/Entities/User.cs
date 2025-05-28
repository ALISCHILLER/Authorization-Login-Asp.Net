using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Domain.Enums;
using Authorization_Login_Asp.Net.Domain.ValueObjects;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Domain.Entities
{
    /// <summary>
    /// مدل کاربر سیستم
    /// نماینده جدول Users در دیتابیس
    /// </summary>
    public class User
    {
        /// <summary>
        /// کلید اصلی (Primary Key)
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری یکتا برای ورود به سیستم
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل کاربر به صورت Value Object (مقدار پیچیده)
        /// برای ذخیره‌سازی باید در کانفیگ EF Core به عنوان Owned Entity تعریف شود
        /// </summary>
        [Required]
        public Email Email { get; set; }

        /// <summary>
        /// هش شده رمز عبور برای نگهداری امن در دیتابیس
        /// هیچ‌گاه رمز عبور به صورت متن ساده ذخیره نمی‌شود
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// نام کامل کاربر (نمایشی در پنل یا گزارش‌ها)
        /// </summary>
        [MaxLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// نقش کاربر در سیستم (Admin, User, Manager و غیره)
        /// استفاده از Enum برای خوانایی بهتر و جلوگیری از اشتباهات تایپی
        /// </summary>
        [Required]
        public Enums.RoleType Role { get; set; }

        /// <summary>
        /// تاریخ ایجاد حساب کاربری (ثبت‌نام)
        /// ذخیره به صورت UTC برای یکپارچگی زمانی
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// وضعیت فعال بودن حساب کاربری
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// شماره تلفن کاربر
        /// </summary>
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود کاربر
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل کاربر
        /// </summary>
        [MaxLength(500)]
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// وضعیت تأیید ایمیل کاربر
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن کاربر
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// تنظیمات امنیتی کاربر
        /// </summary>
        public UserSecuritySettings SecuritySettings { get; set; }

        /// <summary>
        /// لیست دستگاه‌های متصل کاربر
        /// </summary>
        public ICollection<UserDevice> ConnectedDevices { get; set; }

        /// <summary>
        /// لیست توکن‌های رفرش متعلق به این کاربر
        /// یک کاربر می‌تواند چندین رفرش توکن داشته باشد (مثلاً از چند دستگاه)
        /// مقداردهی اولیه در سازنده برای جلوگیری از خطاهای NullReferenceException
        /// </summary>
        public ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// تاریخ آخرین تغییر رمز عبور
        /// </summary>
        public DateTime? LastPasswordChange { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ناموفق ورود
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// زمان پایان قفل شدن حساب (در صورت قفل شدن)
        /// </summary>
        public DateTime? AccountLockoutEnd { get; set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای (ایمیل، پیامک، اپلیکیشن)
        /// </summary>
        public TwoFactorType? TwoFactorType { get; set; }

        /// <summary>
        /// کلید مخفی برای احراز هویت دو مرحله‌ای
        /// </summary>
        public string TwoFactorSecret { get; set; }

        /// <summary>
        /// لیست کدهای بازیابی برای احراز هویت دو مرحله‌ای
        /// </summary>
        public ICollection<TwoFactorRecoveryCode> RecoveryCodes { get; set; }

        /// <summary>
        /// لیست تاریخچه ورودهای کاربر
        /// </summary>
        public ICollection<LoginHistory> LoginHistory { get; set; }

        /// <summary>
        /// سازنده بدون پارامتر برای EF Core
        /// </summary>
        public User()
        {
            RefreshTokens = new List<RefreshToken>();
            ConnectedDevices = new List<UserDevice>();
            SecuritySettings = new UserSecuritySettings();
            RecoveryCodes = new List<TwoFactorRecoveryCode>();
            LoginHistory = new List<LoginHistory>();
        }

        // Helper methods for security management
        public bool IsAccountLocked()
        {
            return AccountLockoutEnd.HasValue && AccountLockoutEnd.Value > DateTime.UtcNow;
        }

        public void IncrementFailedLoginAttempts()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= SecuritySettings.MaxFailedLoginAttempts)
            {
                LockAccount(SecuritySettings.AccountLockoutDurationMinutes);
            }
        }

        public void ResetFailedLoginAttempts()
        {
            FailedLoginAttempts = 0;
            AccountLockoutEnd = null;
        }

        public void LockAccount(int durationMinutes)
        {
            AccountLockoutEnd = DateTime.UtcNow.AddMinutes(durationMinutes);
        }

        public bool IsPasswordExpired()
        {
            if (!LastPasswordChange.HasValue || !SecuritySettings.PasswordExpiryDate.HasValue)
                return false;

            return DateTime.UtcNow >= SecuritySettings.PasswordExpiryDate.Value;
        }

        public bool RequiresPasswordChange()
        {
            return SecuritySettings.RequirePasswordChange || IsPasswordExpired();
        }

        // Helper methods for device management
        public void AddDevice(UserDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            device.UserId = Id;
            device.CreatedAt = DateTime.UtcNow;
            device.LastUsedAt = DateTime.UtcNow;
            device.IsActive = true;

            ConnectedDevices.Add(device);
        }

        public void RemoveDevice(Guid deviceId)
        {
            var device = ConnectedDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.IsActive = false;
                device.LastUsedAt = DateTime.UtcNow;
            }
        }

        public void UpdateDeviceLastUsed(Guid deviceId)
        {
            var device = ConnectedDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.LastUsedAt = DateTime.UtcNow;
            }
        }

        public IEnumerable<UserDevice> GetActiveDevices()
        {
            return ConnectedDevices.Where(d => d.IsActive);
        }

        // Helper methods for two-factor authentication
        public void EnableTwoFactor(TwoFactorType type, string secret)
        {
            TwoFactorEnabled = true;
            TwoFactorType = type;
            TwoFactorSecret = secret;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorType = null;
            TwoFactorSecret = null;
            RecoveryCodes.Clear();
        }

        public void AddRecoveryCode(string code)
        {
            var recoveryCode = new TwoFactorRecoveryCode
            {
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsUsed = false
            };

            RecoveryCodes.Add(recoveryCode);
        }

        public bool UseRecoveryCode(string code)
        {
            var recoveryCode = RecoveryCodes.FirstOrDefault(rc => 
                rc.Code == code && 
                !rc.IsUsed && 
                rc.ExpiresAt > DateTime.UtcNow);

            if (recoveryCode != null)
            {
                recoveryCode.IsUsed = true;
                recoveryCode.UsedAt = DateTime.UtcNow;
                return true;
            }

            return false;
        }

        public void ClearExpiredRecoveryCodes()
        {
            var expiredCodes = RecoveryCodes.Where(rc => 
                rc.ExpiresAt <= DateTime.UtcNow || 
                rc.IsUsed).ToList();

            foreach (var code in expiredCodes)
            {
                RecoveryCodes.Remove(code);
            }
        }

        /// <summary>
        /// تنظیم رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور جدید</param>
        /// <param name="passwordHasher">سرویس هش کردن رمز عبور</param>
        public async Task SetPasswordAsync(string password, IPasswordHasher passwordHasher)
        {
            var (hash, salt) = await passwordHasher.HashPasswordAsync(password);
            PasswordHash = hash;
            SecuritySettings.PasswordSalt = salt;
            LastPasswordChange = DateTime.UtcNow;
        }

        /// <summary>
        /// بررسی صحت رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <param name="passwordHasher">سرویس هش کردن رمز عبور</param>
        /// <returns>نتیجه بررسی</returns>
        public async Task<bool> VerifyPasswordAsync(string password, IPasswordHasher passwordHasher)
        {
            return await passwordHasher.VerifyPasswordAsync(password, PasswordHash, SecuritySettings.PasswordSalt);
        }

        /// <summary>
        /// تنظیم کلید محرمانه احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="secret">کلید محرمانه</param>
        public void SetTwoFactorSecret(string secret)
        {
            TwoFactorSecret = secret;
            TwoFactorEnabled = true;
        }

        /// <summary>
        /// تنظیم کدهای بازیابی
        /// </summary>
        /// <param name="codes">کدهای بازیابی</param>
        public void SetRecoveryCodes(IEnumerable<string> codes)
        {
            RecoveryCodes.Clear();
            foreach (var code in codes)
            {
                AddRecoveryCode(code);
            }
        }

        /// <summary>
        /// بررسی صحت کد بازیابی
        /// </summary>
        /// <param name="code">کد بازیابی</param>
        /// <returns>نتیجه بررسی</returns>
        public bool ValidateRecoveryCode(string code)
        {
            var recoveryCode = RecoveryCodes.FirstOrDefault(rc => rc.Code == code && !rc.IsUsed && rc.ExpiresAt > DateTime.UtcNow);
            if (recoveryCode != null)
            {
                recoveryCode.IsUsed = true;
                recoveryCode.UsedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        /// <summary>
        /// باطل کردن توکن رفرش
        /// </summary>
        public void InvalidateRefreshToken()
        {
            RefreshTokens.Clear();
        }
    }
}
