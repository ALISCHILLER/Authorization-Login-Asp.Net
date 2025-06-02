using System;
using System.Security.Cryptography;
using System.Text;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار توکن امنیتی
    /// </summary>
    public class SecurityToken
    {
        private const int TokenLength = 32;
        private const string TokenChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

        /// <summary>
        /// مقدار توکن
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// زمان ایجاد توکن
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// زمان انقضای توکن
        /// </summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="token">مقدار توکن</param>
        /// <param name="expiresIn">مدت زمان اعتبار به دقیقه</param>
        public SecurityToken(string token, int expiresIn = 30)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            if (token.Length != TokenLength)
                throw new ArgumentException("طول توکن نامعتبر است");

            Value = token;
            CreatedAt = DateTime.UtcNow;
            ExpiresAt = CreatedAt.AddMinutes(expiresIn);
        }

        /// <summary>
        /// ایجاد توکن جدید
        /// </summary>
        /// <param name="expiresIn">مدت زمان اعتبار به دقیقه</param>
        public static SecurityToken CreateNew(int expiresIn = 30)
        {
            var token = GenerateToken();
            return new SecurityToken(token, expiresIn);
        }

        /// <summary>
        /// بررسی اعتبار توکن
        /// </summary>
        public bool IsValid()
        {
            return DateTime.UtcNow <= ExpiresAt;
        }

        /// <summary>
        /// تولید توکن تصادفی
        /// </summary>
        private static string GenerateToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[TokenLength];
            rng.GetBytes(tokenBytes);

            var token = new StringBuilder(TokenLength);
            foreach (var b in tokenBytes)
            {
                token.Append(TokenChars[b % TokenChars.Length]);
            }

            return token.ToString();
        }

        /// <summary>
        /// تبدیل ضمنی به رشته
        /// </summary>
        public static implicit operator string(SecurityToken token) => token?.Value;

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is SecurityToken other)
                return Value.Equals(other.Value, StringComparison.Ordinal);
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