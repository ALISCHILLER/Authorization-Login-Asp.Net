using System;
using System.Numerics;
using System.Text;

namespace Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس تبدیل به فرمت Base62
    /// </summary>
    public static class Base62Encoding
    {
        private const string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private static readonly BigInteger Base62 = new BigInteger(62);

        /// <summary>
        /// تبدیل آرایه بایت به رشته Base62
        /// </summary>
        public static string Encode(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "داده نمی‌تواند خالی باشد");

            if (data.Length == 0)
                return string.Empty;

            var value = new BigInteger(data.Reverse().Concat(new byte[] { 0 }).ToArray());
            var result = new StringBuilder();

            while (value > 0)
            {
                var remainder = (int)(value % Base62);
                value /= Base62;
                result.Insert(0, Base62Chars[remainder]);
            }

            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result.Insert(0, Base62Chars[0]);
            }

            return result.ToString();
        }

        /// <summary>
        /// تبدیل رشته Base62 به آرایه بایت
        /// </summary>
        public static byte[] Decode(string base62)
        {
            if (string.IsNullOrWhiteSpace(base62))
                throw new ArgumentException("رشته Base62 نمی‌تواند خالی باشد", nameof(base62));

            var value = BigInteger.Zero;
            var leadingZeros = 0;

            for (int i = 0; i < base62.Length; i++)
            {
                var c = base62[i];
                var digit = Base62Chars.IndexOf(c);
                if (digit == -1)
                    throw new FormatException($"کاراکتر نامعتبر '{c}' در رشته Base62");

                value = value * Base62 + digit;
                if (c == Base62Chars[0])
                    leadingZeros++;
            }

            var bytes = value.ToByteArray().Reverse().ToArray();
            var result = new byte[leadingZeros + bytes.Length];
            Array.Copy(bytes, 0, result, leadingZeros, bytes.Length);

            return result;
        }

        /// <summary>
        /// بررسی معتبر بودن رشته Base62
        /// </summary>
        public static bool IsValid(string base62)
        {
            if (string.IsNullOrWhiteSpace(base62))
                return false;

            foreach (var c in base62)
            {
                if (Base62Chars.IndexOf(c) == -1)
                    return false;
            }

            return true;
        }
    }
} 