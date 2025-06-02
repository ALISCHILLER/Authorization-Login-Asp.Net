using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using BCrypt.Net;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// مدل کاربر سیستم
    /// این کلاس نماینده جدول Users در دیتابیس است و شامل اطلاعات و رفتارهای مرتبط با کاربر است
    /// </summary>
    public class User : AggregateRoot
    {
        /// <summary>
        /// نام کاربری یکتا برای ورود به سیستم
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Username { get; private set; }

        /// <summary>
        /// ایمیل کاربر به صورت Value Object
        /// </summary>
        [Required]
        public Email Email { get; private set; }

        /// <summary>
        /// هش شده رمز عبور برای نگهداری امن در دیتابیس
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; private set; }

        /// <summary>
        /// نام کامل کاربر
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; private set; }

        /// <summary>
        /// نام کاربر
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; private set; }

        /// <summary>
        /// نام خانوادگی کاربر
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; private set; }

        /// <summary>
        /// شماره تلفن کاربر
        /// </summary>
        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; private set; }

        /// <summary>
        /// آدرس تصویر پروفایل کاربر
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ProfileImageUrl { get; private set; }

        /// <summary>
        /// وضعیت فعال بودن حساب کاربری
        /// </summary>
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// وضعیت تأیید ایمیل کاربر
        /// </summary>
        public bool IsEmailVerified { get; private set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن کاربر
        /// </summary>
        public bool IsPhoneVerified { get; private set; }

        /// <summary>
        /// تاریخ آخرین ورود کاربر
        /// </summary>
        public DateTime? LastLoginAt { get; private set; }

        /// <summary>
        /// تاریخ آخرین تغییر رمز عبور
        /// </summary>
        public DateTime? LastPasswordChange { get; private set; }

        /// <summary>
        /// تعداد تلاش‌های ناموفق ورود
        /// </summary>
        public int FailedLoginAttempts { get; private set; }

        /// <summary>
        /// زمان پایان قفل شدن حساب
        /// </summary>
        public DateTime? AccountLockoutEnd { get; private set; }

        /// <summary>
        /// فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool TwoFactorEnabled { get; private set; }

        /// <summary>
        /// روش احراز هویت دو مرحله‌ای
        /// </summary>
        public TwoFactorType? TwoFactorType { get; private set; }

        /// <summary>
        /// کلید مخفی برای احراز هویت دو مرحله‌ای
        /// </summary>
        [Required]
        public string TwoFactorSecret { get; private set; }

        /// <summary>
        /// تنظیمات امنیتی کاربر
        /// </summary>
        public UserSecuritySettings SecuritySettings { get; private set; }

        /// <summary>
        /// وضعیت قفل بودن حساب
        /// </summary>
        public AccountLockStatus LockStatus { get; private set; } = AccountLockStatus.Create();

        /// <summary>
        /// نقش اصلی کاربر
        /// </summary>
        public RoleType Role { get; private set; }

        /// <summary>
        /// ارتباط‌های این کاربر با نقش‌ها
        /// </summary>
        private readonly List<UserRole> _userRoles = new();
        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public virtual IReadOnlyCollection<Role> Roles => _userRoles.Select(ur => ur.Role).ToList().AsReadOnly();

        /// <summary>
        /// نقش اصلی کاربر
        /// </summary>
        public virtual Role PrimaryRole => _userRoles.OrderBy(ur => ur.CreatedAt).FirstOrDefault()?.Role;

        /// <summary>
        /// دستگاه‌های متصل کاربر
        /// </summary>
        private readonly List<UserDevice> _userDevices = new();
        public virtual IReadOnlyCollection<UserDevice> UserDevices => _userDevices.AsReadOnly();

        /// <summary>
        /// توکن‌های رفرش کاربر
        /// </summary>
        private readonly List<RefreshToken> _refreshTokens = new();
        public virtual IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        /// <summary>
        /// آخرین توکن رفرش کاربر
        /// </summary>
        public virtual RefreshToken LastRefreshToken => _refreshTokens.OrderByDescending(rt => rt.CreatedAt).FirstOrDefault();

        /// <summary>
        /// کدهای بازیابی برای احراز هویت دو مرحله‌ای
        /// </summary>
        private readonly List<TwoFactorRecoveryCode> _recoveryCodes = new();
        public virtual IReadOnlyCollection<TwoFactorRecoveryCode> RecoveryCodes => _recoveryCodes.AsReadOnly();

        /// <summary>
        /// تاریخچه ورودهای کاربر
        /// </summary>
        private readonly List<LoginHistory> _loginHistory = new();
        public virtual IReadOnlyCollection<LoginHistory> LoginHistory => _loginHistory.AsReadOnly();

        /// <summary>
        /// سازنده پیش‌فرض برای EF Core
        /// </summary>
        protected User() { }

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="email">ایمیل</param>
        /// <param name="firstName">نام</param>
        /// <param name="lastName">نام خانوادگی</param>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="password">رمز عبور</param>
        /// <returns>نمونه جدید از User</returns>
        public static User Create(
            string username,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نمی‌تواند خالی باشد", nameof(lastName));
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نمی‌تواند خالی باشد", nameof(phoneNumber));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username.Trim(),
                Email = new Email(email.Trim()),
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                FullName = $"{firstName.Trim()} {lastName.Trim()}",
                PhoneNumber = phoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                ProfileImageUrl = "/images/default-profile.png",
                TwoFactorSecret = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = RoleType.User,
                SecuritySettings = new UserSecuritySettings()
            };

            return user;
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        /// <param name="firstName">نام جدید</param>
        /// <param name="lastName">نام خانوادگی جدید</param>
        /// <param name="phoneNumber">شماره تلفن جدید</param>
        /// <param name="profileImageUrl">آدرس تصویر پروفایل جدید</param>
        public void UpdateProfile(string firstName, string lastName, string profileImageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("نام خانوادگی نمی‌تواند خالی باشد", nameof(lastName));
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("شماره تلفن نمی‌تواند خالی باشد", nameof(phoneNumber));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            FullName = $"{firstName.Trim()} {lastName.Trim()}";
            PhoneNumber = phoneNumber.Trim();
            if (!string.IsNullOrWhiteSpace(profileImageUrl))
                ProfileImageUrl = profileImageUrl.Trim();

            Update();
        }

        /// <summary>
        /// به‌روزرسانی ایمیل کاربر
        /// </summary>
        /// <param name="email">ایمیل جدید</param>
        public void UpdateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            Email = new Email(email.Trim());
            IsEmailVerified = false;
            Update();
        }

        /// <summary>
        /// تأیید ایمیل کاربر
        /// </summary>
        public void VerifyEmail()
        {
            IsEmailVerified = true;
            Update();
        }

        /// <summary>
        /// تأیید شماره تلفن کاربر
        /// </summary>
        public void VerifyPhone()
        {
            IsPhoneVerified = true;
            Update();
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال کاربر
        /// </summary>
        /// <param name="isActive">وضعیت جدید</param>
        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            Update();
        }

        /// <summary>
        /// ثبت ورود موفق کاربر
        /// </summary>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        public void RecordSuccessfulLogin(string deviceInfo)
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            AccountLockoutEnd = null;

            var loginHistory = LoginHistory.Create(Id, deviceInfo, true);
            _loginHistory.Add(loginHistory);

            Update();
        }

        /// <summary>
        /// ثبت تلاش ناموفق ورود
        /// </summary>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        public void RecordFailedLogin(string deviceInfo)
        {
            FailedLoginAttempts++;
            var loginHistory = LoginHistory.Create(Id, deviceInfo, false);
            _loginHistory.Add(loginHistory);

            if (FailedLoginAttempts >= SecuritySettings.MaxFailedLoginAttempts)
            {
                LockAccount(SecuritySettings.LockoutDurationMinutes);
            }

            Update();
        }

        /// <summary>
        /// قفل کردن حساب کاربری
        /// </summary>
        /// <param name="durationMinutes">مدت زمان قفل شدن به دقیقه</param>
        public void LockAccount(int durationMinutes)
        {
            if (durationMinutes <= 0)
                throw new ArgumentException("مدت زمان قفل شدن باید بزرگتر از صفر باشد", nameof(durationMinutes));

            AccountLockoutEnd = DateTime.UtcNow.AddMinutes(durationMinutes);
            LockStatus = AccountLockStatus.CreateLocked("تعداد تلاش‌های ناموفق بیش از حد مجاز");
            Update();
        }

        /// <summary>
        /// باز کردن قفل حساب کاربری
        /// </summary>
        public void UnlockAccount()
        {
            AccountLockoutEnd = null;
            FailedLoginAttempts = 0;
            LockStatus = AccountLockStatus.CreateUnlocked();
            Update();
        }

        /// <summary>
        /// فعال کردن احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="type">نوع احراز هویت</param>
        /// <param name="secret">کلید مخفی</param>
        public void EnableTwoFactor(TwoFactorType type, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("کلید مخفی نمی‌تواند خالی باشد", nameof(secret));

            TwoFactorEnabled = true;
            TwoFactorType = type;
            TwoFactorSecret = secret;
            Update();
        }

        /// <summary>
        /// غیرفعال کردن احراز هویت دو مرحله‌ای
        /// </summary>
        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorType = null;
            TwoFactorSecret = Guid.NewGuid().ToString("N");
            _recoveryCodes.Clear();
            Update();
        }

        /// <summary>
        /// افزودن کد بازیابی
        /// </summary>
        /// <param name="code">کد بازیابی</param>
        /// <param name="expiresAt">تاریخ انقضا</param>
        public void AddRecoveryCode(string code, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد بازیابی نمی‌تواند خالی باشد", nameof(code));
            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("تاریخ انقضا باید در آینده باشد", nameof(expiresAt));

            var recoveryCode = TwoFactorRecoveryCode.Create(Id, code, expiresAt);
            _recoveryCodes.Add(recoveryCode);
            Update();
        }

        /// <summary>
        /// استفاده از کد بازیابی
        /// </summary>
        /// <param name="code">کد بازیابی</param>
        /// <returns>آیا کد معتبر و استفاده نشده بود؟</returns>
        public bool UseRecoveryCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد بازیابی نمی‌تواند خالی باشد", nameof(code));

            var recoveryCode = _recoveryCodes.FirstOrDefault(rc =>
                rc.Code == code &&
                !rc.IsUsed &&
                rc.ExpiresAt > DateTime.UtcNow);

            if (recoveryCode == null)
                return false;

            recoveryCode.Use();
            Update();
            return true;
        }

        /// <summary>
        /// افزودن دستگاه جدید
        /// </summary>
        /// <param name="device">اطلاعات دستگاه</param>
        public void AddDevice(UserDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (!_userDevices.Any(d => d.DeviceId == device.DeviceId))
            {
                device.UserId = Id;
                _userDevices.Add(device);
                Update();
            }
        }

        /// <summary>
        /// حذف دستگاه
        /// </summary>
        /// <param name="deviceId">شناسه دستگاه</param>
        public void RemoveDevice(Guid deviceId)
        {
            var device = _userDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device != null)
            {
                _userDevices.Remove(device);
                Update();
            }
        }

        /// <summary>
        /// به‌روزرسانی آخرین استفاده از دستگاه
        /// </summary>
        /// <param name="deviceId">شناسه دستگاه</param>
        public void UpdateDeviceLastUsed(Guid deviceId)
        {
            var device = _userDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            if (device != null)
            {
                device.UpdateLastUsed();
                Update();
            }
        }

        /// <summary>
        /// دریافت دستگاه‌های فعال
        /// </summary>
        /// <returns>لیست دستگاه‌های فعال</returns>
        public IEnumerable<UserDevice> GetActiveDevices()
        {
            return _userDevices.Where(d => d.IsActive).ToList();
        }

        /// <summary>
        /// افزودن توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        public void AddRefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            refreshToken.UserId = Id;
            _refreshTokens.Add(refreshToken);
            Update();
        }

        /// <summary>
        /// حذف توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        public void RemoveRefreshToken(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            _refreshTokens.Remove(refreshToken);
            Update();
        }

        /// <summary>
        /// باطل کردن همه توکن‌های رفرش
        /// </summary>
        public void RevokeAllRefreshTokens()
        {
            foreach (var token in _refreshTokens)
            {
                token.Revoke();
            }
            Update();
        }

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور جدید</param>
        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            LastPasswordChange = DateTime.UtcNow;
            Update();
        }

        /// <summary>
        /// بررسی رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <returns>آیا رمز عبور صحیح است؟</returns>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        /// <summary>
        /// افزودن نقش به کاربر
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        public void AddRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!_userRoles.Any(ur => ur.RoleId == role.Id))
            {
                var userRole = UserRole.Create(Id, role.Id);
                _userRoles.Add(userRole);
                Update();
            }
        }

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        public void RemoveRole(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole != null)
            {
                _userRoles.Remove(userRole);
                Update();
            }
        }

        /// <summary>
        /// بررسی وجود نقش در کاربر
        /// </summary>
        /// <param name="roleName">نام نقش</param>
        /// <returns>آیا کاربر دارای این نقش است؟</returns>
        public bool HasRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(roleName));

            return _userRoles.Any(ur => ur.Role.Name.Equals(roleName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// بررسی وجود نقش در کاربر
        /// </summary>
        /// <param name="role">نقش مورد نظر</param>
        /// <returns>آیا کاربر دارای این نقش است؟</returns>
        public bool HasRole(Role role)
        {
            return role != null && _userRoles.Any(ur => ur.RoleId == role.Id);
        }

        /// <summary>
        /// پاک کردن همه نقش‌های کاربر
        /// </summary>
        public void ClearRoles()
        {
            _userRoles.Clear();
            Update();
        }

        /// <summary>
        /// افزودن چند نقش به کاربر
        /// </summary>
        /// <param name="roles">نقش‌های مورد نظر</param>
        public void AddRoles(IEnumerable<Role> roles)
        {
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            foreach (var role in roles)
            {
                AddRole(role);
            }
        }

        /// <summary>
        /// دریافت همه نقش‌های کاربر
        /// </summary>
        /// <returns>لیست نقش‌های کاربر</returns>
        public IEnumerable<Role> GetRoles()
        {
            return _userRoles.Select(ur => ur.Role).ToList();
        }
    }
}
