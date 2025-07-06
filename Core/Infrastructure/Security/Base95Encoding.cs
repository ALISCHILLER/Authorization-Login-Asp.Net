using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base95
    /// </summary>
    public static class Base95Encoding
    {
        private const string Base95Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ \t";
        private const int Base95Bits = 13;
        private const int Base95Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base95
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

                while (bitsLeft >= Base95Bits)
                {
                    var index = (buffer >> (bitsLeft - Base95Bits)) & Base95Mask;
                    result.Append(Base95Chars[index]);
                    bitsLeft -= Base95Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base95Bits - bitsLeft);
                var index = buffer & Base95Mask;
                result.Append(Base95Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base95 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base95)
        {
            if (string.IsNullOrWhiteSpace(base95))
                throw new ArgumentException("رشته Base95 نمی‌تواند خالی باشد", nameof(base95));

            var result = new byte[base95.Length * Base95Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base95)
            {
                var value = Base95Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base95");

                buffer = (buffer << Base95Bits) | value;
                bitsLeft += Base95Bits;

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
        /// بررسی معتبر بودن رشته Base95
        /// </summary>
        public static bool IsValid(string base95)
        {
            if (string.IsNullOrWhiteSpace(base95))
                return false;

            foreach (var c in base95)
            {
                if (Base95Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 