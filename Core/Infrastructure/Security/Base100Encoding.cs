using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base100
    /// </summary>
    public static class Base100Encoding
    {
        private const string Base100Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'\\ \t\n\r\f\v\0";
        private const int Base100Bits = 13;
        private const int Base100Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base100
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

                while (bitsLeft >= Base100Bits)
                {
                    var index = (buffer >> (bitsLeft - Base100Bits)) & Base100Mask;
                    result.Append(Base100Chars[index]);
                    bitsLeft -= Base100Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base100Bits - bitsLeft);
                var index = buffer & Base100Mask;
                result.Append(Base100Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base100 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base100)
        {
            if (string.IsNullOrWhiteSpace(base100))
                throw new ArgumentException("رشته Base100 نمی‌تواند خالی باشد", nameof(base100));

            var result = new byte[base100.Length * Base100Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base100)
            {
                var value = Base100Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base100");

                buffer = (buffer << Base100Bits) | value;
                bitsLeft += Base100Bits;

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
        /// بررسی معتبر بودن رشته Base100
        /// </summary>
        public static bool IsValid(string base100)
        {
            if (string.IsNullOrWhiteSpace(base100))
                return false;

            foreach (var c in base100)
            {
                if (Base100Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 