using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using BCrypt.Net;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای رمز عبور
    /// این کلاس شامل اعتبارسنجی، هش کردن و مدیریت رمز عبور است
    /// </summary>
    public class Password : ValueObject
    {
        [Required]
        [MaxLength(100)]
        public string Hash { get; private set; }

        public DateTime? LastChange { get; private set; }

        protected Password() { }

        public static Password Create(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            if (!IsValidPassword(password))
                throw new ArgumentException("رمز عبور باید حداقل 8 کاراکتر و شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد", nameof(password));

            return new Password
            {
                Hash = BCrypt.Net.BCrypt.HashPassword(password),
                LastChange = DateTime.UtcNow
            };
        }

        public static Password FromHash(string hash, DateTime? lastChange = null)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("هش رمز عبور نمی‌تواند خالی باشد", nameof(hash));

            return new Password
            {
                Hash = hash,
                LastChange = lastChange
            };
        }

        private static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // حداقل 8 کاراکتر
            if (password.Length < 8)
                return false;

            // حداقل یک حرف بزرگ
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // حداقل یک حرف کوچک
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            // حداقل یک عدد
            if (!Regex.IsMatch(password, @"\d"))
                return false;

            // حداقل یک کاراکتر خاص
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]"))
                return false;

            return true;
        }

        public bool Verify(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("رمز عبور نمی‌تواند خالی باشد", nameof(password));

            return BCrypt.Net.BCrypt.Verify(password, Hash);
        }

        public void Change(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("رمز عبور جدید نمی‌تواند خالی باشد", nameof(newPassword));

            if (!IsValidPassword(newPassword))
                throw new ArgumentException("رمز عبور جدید باید حداقل 8 کاراکتر و شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد", nameof(newPassword));

            Hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            LastChange = DateTime.UtcNow;
        }

        public bool IsExpired(int maxAgeDays)
        {
            if (!LastChange.HasValue)
                return false;

            return (DateTime.UtcNow - LastChange.Value).TotalDays > maxAgeDays;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Hash;
            yield return LastChange;
        }
    }
} 