using System;
using System.Text.RegularExpressions;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار نام کاربری
    /// </summary>
    public class Username
    {
        private const string UsernamePattern = @"^[a-zA-Z][a-zA-Z0-9_]{3,19}$";
        private const int MinLength = 4;
        private const int MaxLength = 20;

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="username">نام کاربری</param>
        public Username(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            if (!IsValid(username))
                throw new ArgumentException($"نام کاربری باید بین {MinLength} تا {MaxLength} کاراکتر باشد و فقط شامل حروف انگلیسی، اعداد و کاراکتر _ باشد");

            Value = username.ToLowerInvariant();
        }

        /// <summary>
        /// بررسی اعتبار نام کاربری
        /// </summary>
        public static bool IsValid(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < MinLength || username.Length > MaxLength)
                return false;

            return Regex.IsMatch(username, UsernamePattern);
        }

        /// <summary>
        /// تبدیل ضمنی به رشته
        /// </summary>
        public static implicit operator string(Username username) => username?.Value;

        /// <summary>
        /// تبدیل ضمنی از رشته
        /// </summary>
        public static implicit operator Username(string username) => new(username);

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Username other)
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// تبدیل به رشته
        /// </summary>
        public override string ToString() => Value;
    }
} 