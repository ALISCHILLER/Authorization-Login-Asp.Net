using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Security
{
    /// <summary>
    /// کلاس مدیریت رمز عبور با استفاده از PBKDF2
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private readonly ILogger<PasswordHasher> _logger;
        private readonly int _iterations = 10000;
        private readonly int _saltSize = 16;
        private readonly int _keySize = 32;
        private const string VERSION_PREFIX = "v1$";

        public PasswordHasher(ILogger<PasswordHasher> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// هش کردن رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <returns>هش رمز عبور</returns>
        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException(nameof(password));

                // بررسی قدرت رمز عبور
                if (!IsPasswordStrong(password))
                {
                    throw new PasswordValidationException("Password does not meet strength requirements");
                }

                // تولید نمک تصادفی
                byte[] salt = new byte[_saltSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                // هش کردن رمز عبور با PBKDF2
                byte[] hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: _iterations,
                    numBytesRequested: _keySize);

                // ترکیب نمک و هش
                byte[] hashBytes = new byte[_saltSize + _keySize];
                Array.Copy(salt, 0, hashBytes, 0, _saltSize);
                Array.Copy(hash, 0, hashBytes, _saltSize, _keySize);

                // اضافه کردن نسخه الگوریتم
                return VERSION_PREFIX + Convert.ToBase64String(hashBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw new PasswordHashingException("Failed to hash password", ex);
            }
        }

        /// <summary>
        /// بررسی تطابق رمز عبور با هش
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <param name="hashedPassword">هش رمز عبور</param>
        /// <returns>نتیجه بررسی</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException(nameof(password));

                if (string.IsNullOrEmpty(hashedPassword))
                    throw new ArgumentNullException(nameof(hashedPassword));

                // بررسی نسخه الگوریتم
                if (!hashedPassword.StartsWith(VERSION_PREFIX))
                {
                    _logger.LogWarning("Attempted to verify password with invalid hash format");
                    return false;
                }

                // حذف پیشوند نسخه
                string hashWithoutVersion = hashedPassword.Substring(VERSION_PREFIX.Length);

                // تبدیل هش به آرایه بایت
                byte[] hashBytes = Convert.FromBase64String(hashWithoutVersion);

                // استخراج نمک
                byte[] salt = new byte[_saltSize];
                Array.Copy(hashBytes, 0, salt, 0, _saltSize);

                // هش کردن رمز عبور ورودی
                byte[] hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: _iterations,
                    numBytesRequested: _keySize);

                // مقایسه هش‌ها
                for (int i = 0; i < _keySize; i++)
                {
                    if (hashBytes[i + _saltSize] != hash[i])
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                throw new PasswordHashingException("Failed to verify password", ex);
            }
        }

        /// <summary>
        /// بررسی قدرت رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <returns>نتیجه بررسی</returns>
        public bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // حداقل 8 کاراکتر
            if (password.Length < 8)
                return false;

            // حداقل یک حرف بزرگ
            if (!Regex.IsMatch(password, "[A-Z]"))
                return false;

            // حداقل یک حرف کوچک
            if (!Regex.IsMatch(password, "[a-z]"))
                return false;

            // حداقل یک عدد
            if (!Regex.IsMatch(password, "[0-9]"))
                return false;

            // حداقل یک کاراکتر خاص
            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
                return false;

            return true;
        }

        /// <summary>
        /// بررسی تکراری نبودن رمز عبور
        /// </summary>
        /// <param name="password">رمز عبور</param>
        /// <param name="previousPasswords">رمزهای عبور قبلی</param>
        /// <returns>نتیجه بررسی</returns>
        public bool IsPasswordUnique(string password, IEnumerable<string> previousPasswords)
        {
            try
            {
                foreach (var previousHash in previousPasswords)
                {
                    if (VerifyPassword(password, previousHash))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password uniqueness");
                throw new PasswordHashingException("Failed to check password uniqueness", ex);
            }
        }
    }

    public class PasswordHashingException : Exception
    {
        public PasswordHashingException(string message) : base(message) { }
        public PasswordHashingException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    public class PasswordValidationException : Exception
    {
        public PasswordValidationException(string message) : base(message) { }
        public PasswordValidationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 