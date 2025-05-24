using System;
using System.Text.RegularExpressions;

namespace Authorization_Login_Asp.Net.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای ایمیل
    /// اعتبارسنجی و منطق مرتبط با ایمیل در این کلاس نگهداری می‌شود.
    /// </summary>
    public class Email : IEquatable<Email>
    {
        /// <summary>
        /// مقدار ایمیل (رشته معتبر)
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Regex برای اعتبارسنجی ایمیل
        /// </summary>
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// سازنده خصوصی فقط برای EF Core یا Serialization
        /// </summary>
        private Email() { }

        /// <summary>
        /// سازنده عمومی با اعتبارسنجی ایمیل
        /// </summary>
        /// <param name="email">رشته ایمیل ورودی</param>
        /// <exception cref="ArgumentException">اگر ایمیل نامعتبر باشد</exception>
        public Email(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد.", nameof(email));

            if (!EmailRegex.IsMatch(email))
                throw new ArgumentException("ایمیل وارد شده معتبر نیست.", nameof(email));

            Value = email.ToLowerInvariant();
        }

        /// <summary>
        /// مقایسه دو Email برای برابری
        /// </summary>
        public bool Equals(Email other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object obj) => Equals(obj as Email);

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;

        /// <summary>
        /// عملگر تبدیل صریح از Email به string
        /// </summary>
        public static explicit operator string(Email email) => email.Value;

        /// <summary>
        /// عملگر تبدیل صریح از string به Email
        /// </summary>
        public static explicit operator Email(string email) => new Email(email);
    }
}
