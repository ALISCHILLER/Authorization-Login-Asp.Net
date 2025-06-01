using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using BCrypt.Net;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
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
        [MaxLength(100)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// نام کامل کاربر (نمایشی در پنل یا گزارش‌ها)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

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
        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود کاربر
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل کاربر
        /// </summary>
        [Required]
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
        /// دستگاه‌های متصل کاربر
        /// </summary>
        public virtual ICollection<UserDevice> UserDevices { get; private set; }

        /// <summary>
        /// لیست توکن‌های رفرش کاربر
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        /// <summary>
        /// آخرین توکن رفرش کاربر
        /// </summary>
        public RefreshToken RefreshToken => RefreshTokens?.OrderByDescending(rt => rt.CreatedAt).FirstOrDefault();

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
        [Required]
        public string TwoFactorSecret { get; set; }

        /// <summary>
        /// لیست کدهای بازیابی برای احراز هویت دو مرحله‌ای
        /// </summary>
        public virtual ICollection<TwoFactorRecoveryCode> RecoveryCodes { get; private set; }

        /// <summary>
        /// لیست تاریخچه ورودهای کاربر
        /// </summary>
        public virtual ICollection<LoginHistory> LoginHistory { get; private set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی کاربر
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// وضعیت قفل بودن حساب
        /// </summary>
        public AccountLockStatus LockStatus { get; private set; } = AccountLockStatus.Create();

        /// <summary>
        /// نقش اصلی کاربر
        /// </summary>
        public RoleType Role { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public virtual ICollection<Role> Roles => UserRoles?.Select(ur => ur.Role).ToList() ?? new List<Role>();

        /// <summary>
        /// نقش اصلی کاربر
        /// </summary>
        public Role PrimaryRole => UserRoles?.OrderBy(ur => ur.CreatedAt).FirstOrDefault()?.Role;

        /// <summary>
        /// سازنده بدون پارامتر برای EF Core
        /// </summary>
        public User()
        {
            Id = Guid.NewGuid();
            Username = "default";
            Email = new Email("default@example.com");
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("DefaultPassword123!");
            FullName = "Default User";
            PhoneNumber = "00000000000";
            ProfileImageUrl = "/images/default-profile.png";
            TwoFactorSecret = Guid.NewGuid().ToString("N");
            FirstName = "Default";
            LastName = "User";
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            RefreshTokens = new List<RefreshToken>();
            UserDevices = new List<UserDevice>();
            SecuritySettings = new UserSecuritySettings();
            RecoveryCodes = new List<TwoFactorRecoveryCode>();
            LoginHistory = new List<LoginHistory>();
            UserRoles = new List<UserRole>();
            Role = RoleType.User;
        }

        public static User Create(
            string username,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            return new User
            {
                Username = username,
                Email = new Email(email),
                FirstName = firstName,
                LastName = lastName,
                FullName = $"{firstName} {lastName}",
                PhoneNumber = phoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                ProfileImageUrl = "/images/default-profile.png",
                TwoFactorSecret = Guid.NewGuid().ToString("N")
            };
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

            UserDevices.Add(device);
        }

        public void RemoveDevice(Guid deviceId)
        {
            var device = UserDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.IsActive = false;
                device.LastUsedAt = DateTime.UtcNow;
            }
        }

        public void UpdateDeviceLastUsed(Guid deviceId)
        {
            var device = UserDevices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.LastUsedAt = DateTime.UtcNow;
            }
        }

        public IEnumerable<UserDevice> GetActiveDevices()
        {
            return UserDevices.Where(d => d.IsActive);
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

        public void AddRecoveryCode(string code, DateTime expiresAt)
        {
            var recoveryCode = new TwoFactorRecoveryCode
            {
                UserId = Id,
                Code = code,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            RecoveryCodes.Add(recoveryCode);
        }

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

        public void InvalidateRefreshToken()
        {
            RefreshTokens.Clear();
        }

        public void SetLockStatus(AccountLockStatus status)
        {
            LockStatus = status;
            if (status == AccountLockStatus.Locked)
            {
                AccountLockoutEnd = DateTime.UtcNow.AddMinutes(SecuritySettings.AccountLockoutDurationMinutes);
            }
            else
            {
                AccountLockoutEnd = null;
            }
        }

        /// <summary>
        /// تنظیم رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور جدید</param>
        /// <param name="passwordHasher">سرویس هش کردن رمز عبور</param>
        public async Task SetPasswordAsync(string password, IPasswordHasher passwordHasher)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));
            if (passwordHasher == null)
                throw new ArgumentNullException(nameof(passwordHasher));

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
            if (string.IsNullOrWhiteSpace(password))
                return false;
            if (passwordHasher == null)
                throw new ArgumentNullException(nameof(passwordHasher));

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
                AddRecoveryCode(code, DateTime.UtcNow.AddDays(30));
            }
        }

        /// <summary>
        /// افزودن نقش به کاربر
        /// </summary>
        public void AddRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (UserRoles.Any(ur => ur.RoleId == role.Id))
                return;

            UserRoles.Add(new UserRole
            {
                UserId = Id,
                RoleId = role.Id,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        public void RemoveRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }

        /// <summary>
        /// بررسی داشتن نقش
        /// </summary>
        public bool HasRole(string roleName)
        {
            return UserRoles.Any(ur =>
                ur.Role != null &&
                ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// بررسی داشتن نقش
        /// </summary>
        public bool HasRole(Role role)
        {
            return role != null && UserRoles.Any(ur => ur.RoleId == role.Id);
        }

        public bool UseRecoveryCode(string code)
        {
            var recoveryCode = RecoveryCodes.FirstOrDefault(rc =>
                rc.Code == code &&
                !rc.IsUsed &&
                rc.ExpiresAt > DateTime.UtcNow);

            if (recoveryCode == null)
                return false;

            recoveryCode.IsUsed = true;
            recoveryCode.UsedAt = DateTime.UtcNow;
            return true;
        }

        public void AddRefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            RefreshTokens.Add(refreshToken);
        }

        public void RemoveRefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            RefreshTokens.Remove(refreshToken);
        }

        public void RevokeAllRefreshTokens()
        {
            foreach (var token in RefreshTokens.Where(rt => !rt.IsRevoked))
            {
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "User revoked all tokens";
            }
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            LastPasswordChange = DateTime.UtcNow;
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void ClearRoles()
        {
            UserRoles.Clear();
        }

        public void AddRoles(IEnumerable<Role> roles)
        {
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            foreach (var role in roles)
            {
                AddRole(role);
            }
        }

        public IEnumerable<Role> GetRoles()
        {
            return UserRoles?.Select(ur => ur.Role).ToList() ?? new List<Role>();
        }

        public void LockAccount(string reason)
        {
            LockStatus = AccountLockStatus.Locked;
            AccountLockoutEnd = DateTime.UtcNow.AddMinutes(SecuritySettings.AccountLockoutDurationMinutes);
        }

        public void UnlockAccount()
        {
            LockStatus = AccountLockStatus.Unlocked;
            AccountLockoutEnd = null;
            FailedLoginAttempts = 0;
        }
    }
}
