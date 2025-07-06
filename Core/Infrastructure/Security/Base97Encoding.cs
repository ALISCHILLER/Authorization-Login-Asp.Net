using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base97
    /// </summary>
    public static class Base97Encoding
    {
        private const string Base97Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ \t\n\r";
        private const int Base97Bits = 13;
        private const int Base97Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base97
        /// </summary>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "داده نمی‌تواند خالی باشد");

            if (data.Length == 0)
                return string.Empty;

            var result = new StringBuilder();
            var buffer = 0;
            var bitsLeft = 0;

            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;

                while (bitsLeft >= Base97Bits)
                {
                    var index = (buffer >> (bitsLeft - Base97Bits)) & Base97Mask;
                    result.Append(Base97Chars[index]);
                    bitsLeft -= Base97Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base97Bits - bitsLeft);
                var index = buffer & Base97Mask;
                result.Append(Base97Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base97 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base97)
        {
            if (string.IsNullOrWhiteSpace(base97))
                throw new ArgumentException("رشته Base97 نمی‌تواند خالی باشد", nameof(base97));

            var result = new byte[base97.Length * Base97Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base97)
            {
                var value = Base97Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base97");

                buffer = (buffer << Base97Bits) | value;
                bitsLeft += Base97Bits;

                while (bitsLeft >= 8)
                {
                    result[index++] = (byte)((buffer >> (bitsLeft - 8)) & 0xFF);
                    bitsLeft -= 8;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (8 - bitsLeft);
                result[index] = (byte)(buffer & 0xFF);
            }

            return result;
        }

        /// <summary>
        /// بررسی معتبر بودن رشته Base97
        /// </summary>
        public static bool IsValid(string base97)
        {
            if (string.IsNullOrWhiteSpace(base97))
                return false;

            foreach (var c in base97)
            {
                if (Base97Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 