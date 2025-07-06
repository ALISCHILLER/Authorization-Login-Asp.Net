using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base94
    /// </summary>
    public static class Base94Encoding
    {
        private const string Base94Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ ";
        private const int Base94Bits = 13;
        private const int Base94Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base94
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

                while (bitsLeft >= Base94Bits)
                {
                    var index = (buffer >> (bitsLeft - Base94Bits)) & Base94Mask;
                    result.Append(Base94Chars[index]);
                    bitsLeft -= Base94Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base94Bits - bitsLeft);
                var index = buffer & Base94Mask;
                result.Append(Base94Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base94 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base94)
        {
            if (string.IsNullOrWhiteSpace(base94))
                throw new ArgumentException("رشته Base94 نمی‌تواند خالی باشد", nameof(base94));

            var result = new byte[base94.Length * Base94Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base94)
            {
                var value = Base94Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base94");

                buffer = (buffer << Base94Bits) | value;
                bitsLeft += Base94Bits;

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
        /// بررسی معتبر بودن رشته Base94
        /// </summary>
        public static bool IsValid(string base94)
        {
            if (string.IsNullOrWhiteSpace(base94))
                return false;

            foreach (var c in base94)
            {
                if (Base94Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 