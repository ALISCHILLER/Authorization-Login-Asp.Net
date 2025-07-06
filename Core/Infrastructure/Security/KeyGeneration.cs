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

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base58
        /// </summary>
        public static string GenerateRandomKeyBase58(int length)
        {
            var key = GenerateRandomKey(length);
            return Base58Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base62
        /// </summary>
        public static string GenerateRandomKeyBase62(int length)
        {
            var key = GenerateRandomKey(length);
            return Base62Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base85
        /// </summary>
        public static string GenerateRandomKeyBase85(int length)
        {
            var key = GenerateRandomKey(length);
            return Base85Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base91
        /// </summary>
        public static string GenerateRandomKeyBase91(int length)
        {
            var key = GenerateRandomKey(length);
            return Base91Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base92
        /// </summary>
        public static string GenerateRandomKeyBase92(int length)
        {
            var key = GenerateRandomKey(length);
            return Base92Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base93
        /// </summary>
        public static string GenerateRandomKeyBase93(int length)
        {
            var key = GenerateRandomKey(length);
            return Base93Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base94
        /// </summary>
        public static string GenerateRandomKeyBase94(int length)
        {
            var key = GenerateRandomKey(length);
            return Base94Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base95
        /// </summary>
        public static string GenerateRandomKeyBase95(int length)
        {
            var key = GenerateRandomKey(length);
            return Base95Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base96
        /// </summary>
        public static string GenerateRandomKeyBase96(int length)
        {
            var key = GenerateRandomKey(length);
            return Base96Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base97
        /// </summary>
        public static string GenerateRandomKeyBase97(int length)
        {
            var key = GenerateRandomKey(length);
            return Base97Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base98
        /// </summary>
        public static string GenerateRandomKeyBase98(int length)
        {
            var key = GenerateRandomKey(length);
            return Base98Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base99
        /// </summary>
        public static string GenerateRandomKeyBase99(int length)
        {
            var key = GenerateRandomKey(length);
            return Base99Encoding.Encode(key);
        }

        /// <summary>
        /// تولید کلید تصادفی با طول مشخص به صورت رشته Base100
        /// </summary>
        public static string GenerateRandomKeyBase100(int length)
        {
            var key = GenerateRandomKey(length);
            return Base100Encoding.Encode(key);
        }
    }
} 