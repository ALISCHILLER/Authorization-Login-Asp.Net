using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای ایمیل
    /// این کلاس شامل اعتبارسنجی و مدیریت ایمیل است
    /// </summary>
    public class Email : ValueObject
    {
        /// <summary>
        /// آدرس ایمیل
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Value { get; private set; }

        protected Email() { }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if (!IsValidEmail(email))
                throw new ArgumentException("فرمت ایمیل نامعتبر است", nameof(email));

            Value = email.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// سازنده استاتیک
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <returns>نمونه جدید از ایمیل</returns>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            email = email.Trim().ToLowerInvariant();

            if (!IsValidEmail(email))
                throw new ArgumentException("فرمت ایمیل نامعتبر است", nameof(email));

            return new Email { Value = email };
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // استفاده از Regex برای اعتبارسنجی ایمیل
                var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// تبدیل ضمنی به رشته
        /// </summary>
        public static implicit operator string(Email email) => email?.Value;

        /// <summary>
        /// تبدیل ضمنی از رشته
        /// </summary>
        public static explicit operator Email(string email) => Create(email);

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Email other)
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

        /// <summary>
        /// تبدیل به رشته
        /// </summary>
        public override string ToString() => Value;

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}