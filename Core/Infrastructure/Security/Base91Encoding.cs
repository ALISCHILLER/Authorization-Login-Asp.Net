using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base91
    /// </summary>
    public static class Base91Encoding
    {
        private const string Base91Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"";
        private const int Base91Bits = 13;
        private const int Base91Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base91
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

                while (bitsLeft >= Base91Bits)
                {
                    var index = (buffer >> (bitsLeft - Base91Bits)) & Base91Mask;
                    result.Append(Base91Chars[index]);
                    bitsLeft -= Base91Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base91Bits - bitsLeft);
                var index = buffer & Base91Mask;
                result.Append(Base91Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base91 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base91)
        {
            if (string.IsNullOrWhiteSpace(base91))
                throw new ArgumentException("رشته Base91 نمی‌تواند خالی باشد", nameof(base91));

            var result = new byte[base91.Length * Base91Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base91)
            {
                var value = Base91Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base91");

                buffer = (buffer << Base91Bits) | value;
                bitsLeft += Base91Bits;

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
        /// بررسی معتبر بودن رشته Base91
        /// </summary>
        public static bool IsValid(string base91)
        {
            if (string.IsNullOrWhiteSpace(base91))
                return false;

            foreach (var c in base91)
            {
                if (Base91Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 