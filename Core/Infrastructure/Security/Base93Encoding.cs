using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base93
    /// </summary>
    public static class Base93Encoding
    {
        private const string Base93Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\";
        private const int Base93Bits = 13;
        private const int Base93Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base93
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

                while (bitsLeft >= Base93Bits)
                {
                    var index = (buffer >> (bitsLeft - Base93Bits)) & Base93Mask;
                    result.Append(Base93Chars[index]);
                    bitsLeft -= Base93Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base93Bits - bitsLeft);
                var index = buffer & Base93Mask;
                result.Append(Base93Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base93 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base93)
        {
            if (string.IsNullOrWhiteSpace(base93))
                throw new ArgumentException("رشته Base93 نمی‌تواند خالی باشد", nameof(base93));

            var result = new byte[base93.Length * Base93Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base93)
            {
                var value = Base93Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base93");

                buffer = (buffer << Base93Bits) | value;
                bitsLeft += Base93Bits;

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
        /// بررسی معتبر بودن رشته Base93
        /// </summary>
        public static bool IsValid(string base93)
        {
            if (string.IsNullOrWhiteSpace(base93))
                return false;

            foreach (var c in base93)
            {
                if (Base93Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 