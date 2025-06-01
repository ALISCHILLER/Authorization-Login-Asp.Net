using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    /// <summary>
    /// پیاده‌سازی سرویس هش کردن رمز عبور با استفاده از PBKDF2
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasherOptions _options;

        public PasswordHasher(IOptions<PasswordHasherOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// هش کردن رمز عبور با استفاده از PBKDF2
        /// </summary>
        public async Task<(string hash, string salt)> HashPasswordAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            // تولید نمک تصادفی
            byte[] salt = new byte[_options.SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // هش کردن رمز عبور با PBKDF2
            byte[] hash = await Task.Run(() => Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: salt,
                iterations: _options.Iterations,
                hashAlgorithm: HashAlgorithmName.SHA512,
                outputLength: _options.KeySize));

            return (
                hash: Convert.ToBase64String(hash),
                salt: Convert.ToBase64String(salt)
            );
        }

        /// <summary>
        /// بررسی تطابق رمز عبور با هش
        /// </summary>
        public async Task<bool> VerifyPasswordAsync(string password, string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash cannot be empty", nameof(hash));
            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Salt cannot be empty", nameof(salt));

            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] hashBytes = Convert.FromBase64String(hash);

            // هش کردن رمز عبور ورودی با همان پارامترها
            byte[] computedHash = await Task.Run(() => Rfc2898DeriveBytes.Pbkdf2(
                password: Encoding.UTF8.GetBytes(password),
                salt: saltBytes,
                iterations: _options.Iterations,
                hashAlgorithm: HashAlgorithmName.SHA512,
                outputLength: _options.KeySize));

            // مقایسه هش‌ها با استفاده از مقایسه ثابت زمانی
            return CryptographicOperations.FixedTimeEquals(computedHash, hashBytes);
        }

        /// <summary>
        /// بررسی نیاز به بروزرسانی هش
        /// </summary>
        public bool NeedsRehash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentException("Hash cannot be empty", nameof(hash));

            // در حال حاضر، هش‌ها همیشه با آخرین پارامترها تولید می‌شوند
            // این متد برای سازگاری با تغییرات آینده در پارامترهای هش کردن است
            return false;
        }
    }

    /// <summary>
    /// تنظیمات سرویس هش کردن رمز عبور
    /// </summary>
    public class PasswordHasherOptions
    {
        /// <summary>
        /// تعداد تکرارهای PBKDF2
        /// </summary>
        public int Iterations { get; set; } = 310000;

        /// <summary>
        /// اندازه نمک به بایت
        /// </summary>
        public int SaltSize { get; set; } = 16;

        /// <summary>
        /// اندازه کلید خروجی به بایت
        /// </summary>
        public int KeySize { get; set; } = 32;
    }
}