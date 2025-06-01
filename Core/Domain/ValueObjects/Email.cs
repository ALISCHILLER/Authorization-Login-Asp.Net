using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار ایمیل
    /// </summary>
    public class Email
    {
        /// <summary>
        /// آدرس ایمیل
        /// </summary>
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Value { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if (!new EmailAddressAttribute().IsValid(email))
                throw new ArgumentException("Invalid email format", nameof(email));

            Value = email.ToLowerInvariant();
        }

        /// <summary>
        /// سازنده استاتیک
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <returns>نمونه جدید از ایمیل</returns>
        public static Email From(string email)
        {
            return new Email(email);
        }

        /// <summary>
        /// تبدیل ضمنی به رشته
        /// </summary>
        public static implicit operator string(Email email) => email?.Value;

        /// <summary>
        /// تبدیل ضمنی از رشته
        /// </summary>
        public static implicit operator Email(string email) => new(email);

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
    }
}