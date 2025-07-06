using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base92
    /// </summary>
    public static class Base92Encoding
    {
        private const string Base92Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!#$%&()*+,./:;<=>?@[]^_`{|}~\"'";
        private const int Base92Bits = 13;
        private const int Base92Mask = 8191;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base92
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

                while (bitsLeft >= Base92Bits)
                {
                    var index = (buffer >> (bitsLeft - Base92Bits)) & Base92Mask;
                    result.Append(Base92Chars[index]);
                    bitsLeft -= Base92Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base92Bits - bitsLeft);
                var index = buffer & Base92Mask;
                result.Append(Base92Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base92 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base92)
        {
            if (string.IsNullOrWhiteSpace(base92))
                throw new ArgumentException("رشته Base92 نمی‌تواند خالی باشد", nameof(base92));

            var result = new byte[base92.Length * Base92Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base92)
            {
                var value = Base92Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base92");

                buffer = (buffer << Base92Bits) | value;
                bitsLeft += Base92Bits;

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
        /// بررسی معتبر بودن رشته Base92
        /// </summary>
        public static bool IsValid(string base92)
        {
            if (string.IsNullOrWhiteSpace(base92))
                return false;

            foreach (var c in base92)
            {
                if (Base92Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 