using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base99
    /// </summary>
    public static class Base99Encoding
    {
        private const string Base99Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ \t\n\r\f\v";
        private const int Base99Bits = 13;
        private const int Base99Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base99
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

                while (bitsLeft >= Base99Bits)
                {
                    var index = (buffer >> (bitsLeft - Base99Bits)) & Base99Mask;
                    result.Append(Base99Chars[index]);
                    bitsLeft -= Base99Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base99Bits - bitsLeft);
                var index = buffer & Base99Mask;
                result.Append(Base99Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base99 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base99)
        {
            if (string.IsNullOrWhiteSpace(base99))
                throw new ArgumentException("رشته Base99 نمی‌تواند خالی باشد", nameof(base99));

            var result = new byte[base99.Length * Base99Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base99)
            {
                var value = Base99Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base99");

                buffer = (buffer << Base99Bits) | value;
                bitsLeft += Base99Bits;

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
        /// بررسی معتبر بودن رشته Base99
        /// </summary>
        public static bool IsValid(string base99)
        {
            if (string.IsNullOrWhiteSpace(base99))
                return false;

            foreach (var c in base99)
            {
                if (Base99Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 