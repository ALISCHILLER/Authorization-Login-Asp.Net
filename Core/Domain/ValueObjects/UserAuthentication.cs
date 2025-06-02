using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using BCrypt.Net;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    public class UserAuthentication : ValueObject
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; private set; }

        [Required]
        [MaxLength(100)]
        public string PasswordHash { get; private set; }

        public DateTime? LastPasswordChange { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? AccountLockoutEnd { get; private set; }
        public bool TwoFactorEnabled { get; private set; }
        public TwoFactorType? TwoFactorType { get; private set; }
        public string TwoFactorSecret { get; private set; }

        protected UserAuthentication() { }

        public static UserAuthentication Create(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            return new UserAuthentication
            {
                Username = username.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                LastPasswordChange = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                TwoFactorSecret = Guid.NewGuid().ToString("N")
            };
        }

        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            LastPasswordChange = DateTime.UtcNow;
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }

        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;
        }

        public void ResetFailedLoginAttempts()
        {
            FailedLoginAttempts = 0;
        }

        public void LockAccount(int durationMinutes)
        {
            AccountLockoutEnd = DateTime.UtcNow.AddMinutes(durationMinutes);
        }

        public void UnlockAccount()
        {
            AccountLockoutEnd = null;
            FailedLoginAttempts = 0;
        }

        public void EnableTwoFactor(TwoFactorType type, string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("کلید مخفی نمی‌تواند خالی باشد", nameof(secret));

            TwoFactorEnabled = true;
            TwoFactorType = type;
            TwoFactorSecret = secret;
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorType = null;
            TwoFactorSecret = null;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Username;
            yield return PasswordHash;
            yield return LastPasswordChange;
            yield return FailedLoginAttempts;
            yield return AccountLockoutEnd;
            yield return TwoFactorEnabled;
            yield return TwoFactorType;
            yield return TwoFactorSecret;
        }
    }
} 