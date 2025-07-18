using System;
using System.Security.Cryptography;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تولید کلیدهای امن
    /// </summary>
    public static class KeyGeneration
    {
        /// <summary>
        /// تولید کلید تصادفی با طول مشخص
        /// </summary>
        public static byte[] GenerateRandomKey(int length)
        {
            if (length < 1)
                throw new ArgumentException("طول کلید باید بزرگتر از صفر باشد", nameof(length));

            var key = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base64
        /// </summary>
        public static string GenerateRandomKeyBase64(int length)
        {
            var key = GenerateRandomKey(length);
            return Convert.ToBase64String(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته هگز
        /// </summary>
        public static string GenerateRandomKeyHex(int length)
        {
            var key = GenerateRandomKey(length);
            return BitConverter.ToString(key).Replace("-", "");
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base32
        /// </summary>
        public static string GenerateRandomKeyBase32(int length)
        {
            var key = GenerateRandomKey(length);
            return Base32Encoding.ToString(key);
        }

    }
} 