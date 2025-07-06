using System;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base32
    /// </summary>
    public static class Base32Encoding
    {
        private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int Base32Bits = 5;
        private const int Base32Mask = 31;

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base32
        /// </summary>
        public static string ToString(byte[] data)
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

                while (bitsLeft >= Base32Bits)
                {
                    var index = (buffer >> (bitsLeft - Base32Bits)) & Base32Mask;
                    result.Append(Base32Chars[index]);
                    bitsLeft -= Base32Bits;
                }
            }

            if (bitsLeft > 0)
            {
                buffer <<= (Base32Bits - bitsLeft);
                var index = buffer & Base32Mask;
                result.Append(Base32Chars[index]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base32 به آرایه بایت
        /// </summary>
        public static byte[] ToBytes(string base32)
        {
            if (string.IsNullOrWhiteSpace(base32))
                throw new ArgumentException("رشته Base32 نمی‌تواند خالی باشد", nameof(base32));

            base32 = base32.ToUpper();
            var result = new byte[base32.Length * Base32Bits / 8];
            var buffer = 0;
            var bitsLeft = 0;
            var index = 0;

            foreach (var c in base32)
            {
                var value = Base32Chars.IndexOf(c);
                if (value == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base32");

                buffer = (buffer << Base32Bits) | value;
                bitsLeft += Base32Bits;

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
        /// بررسی معتبر بودن رشته Base32
        /// </summary>
        public static bool IsValid(string base32)
        {
            if (string.IsNullOrWhiteSpace(base32))
                return false;

            base32 = base32.ToUpper();
            foreach (var c in base32)
            {
                if (Base32Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 