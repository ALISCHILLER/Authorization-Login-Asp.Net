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
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] DisposableEmailDomains = new[]
        {
            "tempmail.com",
            "throwawaymail.com",
            "mailinator.com",
            "guerrillamail.com",
            "10minutemail.com",
            "yopmail.com"
        };

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

            if (IsDisposableEmail(email))
                throw new ArgumentException("ایمیل موقت مجاز نیست.", nameof(email));

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

        // Helper methods
        public string GetUsername()
        {
            return Value.Split('@')[0];
        }

        public string GetDomain()
        {
            return Value.Split('@')[1];
        }

        public bool IsDisposableEmail(string email)
        {
            var domain = email.Split('@')[1].ToLowerInvariant();
            return Array.Exists(DisposableEmailDomains, d => domain.EndsWith(d));
        }

        public bool IsCorporateEmail()
        {
            var domain = GetDomain();
            return !domain.Contains("gmail.com") &&
                   !domain.Contains("yahoo.com") &&
                   !domain.Contains("hotmail.com") &&
                   !domain.Contains("outlook.com");
        }

        public bool IsValidForRegistration()
        {
            return !IsDisposableEmail(Value) && 
                   (IsCorporateEmail() || Value.Length >= 8);
        }

        public static bool TryParse(string email, out Email result)
        {
            try
            {
                result = new Email(email);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
