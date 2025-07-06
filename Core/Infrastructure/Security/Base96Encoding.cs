using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base96
    /// </summary>
    public static class Base96Encoding
    {
        private const string Base96Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ \t\n";
        private const int Base96Bits = 13;
        private const int Base96Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base96
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

                while (bitsLeft >= Base96Bits)
                {
                    var index = (buffer >> (bitsLeft - Base96Bits)) & Base96Mask;
                    result.Append(Base96Chars[index]);
                    bitsLeft -= Base96Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base96Bits - bitsLeft);
                var index = buffer & Base96Mask;
                result.Append(Base96Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base96 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base96)
        {
            if (string.IsNullOrWhiteSpace(base96))
                throw new ArgumentException("رشته Base96 نمی‌تواند خالی باشد", nameof(base96));

            var result = new byte[base96.Length * Base96Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base96)
            {
                var value = Base96Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base96");

                buffer = (buffer << Base96Bits) | value;
                bitsLeft += Base96Bits;

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
        /// بررسی معتبر بودن رشته Base96
        /// </summary>
        public static bool IsValid(string base96)
        {
            if (string.IsNullOrWhiteSpace(base96))
                return false;

            foreach (var c in base96)
            {
                if (Base96Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 