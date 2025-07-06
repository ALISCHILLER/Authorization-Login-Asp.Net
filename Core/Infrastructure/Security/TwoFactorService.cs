using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Security
{
    public class TwoFactorService : ITwoFactorService
    {
        /// <summary>
        /// تولید کلید مخفی برای احراز هویت دو مرحله‌ای
        /// </summary>
        public async Task<string> GenerateSecretAsync()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return await Task.FromResult(Base32Encoding.ToString(key));
        }

        /// <summary>
        /// تولید کد تایید دو مرحله‌ای
        /// </summary>
        public async Task<string> GenerateCodeAsync(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("کلید مخفی نمی‌تواند خالی باشد", nameof(secret));

            var key = Base32Encoding.ToBytes(secret);
            var counter = (ulong)(DateTime.UtcNow - UnixEpoch).TotalSeconds / 30;
            var counterBytes = BitConverter.GetBytes(counter);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counterBytes);

            var hash = new HMACSHA1(key).ComputeHash(counterBytes);
            var offset = hash[hash.Length - 1] & 0xf;
            var binary = ((hash[offset] & 0x7f) << 24) |
                        ((hash[offset + 1] & 0xff) << 16) |
                        ((hash[offset + 2] & 0xff) << 8) |
                        (hash[offset + 3] & 0xff);

            var otp = binary % 1000000;
            return await Task.FromResult(otp.ToString().PadLeft(6, '0'));
        }

        /// <summary>
        /// بررسی کد تایید دو مرحله‌ای
        /// </summary>
        public async Task<bool> VerifyCodeAsync(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("کلید مخفی نمی‌تواند خالی باشد", nameof(secret));

            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("کد تایید نمی‌تواند خالی باشد", nameof(code));

            if (code.Length != 6 || !code.All(char.IsDigit))
                throw new ArgumentException("کد تایید باید 6 رقم باشد", nameof(code));

            var currentCode = await GenerateCodeAsync(secret);
            return code == currentCode;
        }

        /// <summary>
        /// تولید QR کد برای احراز هویت دو مرحله‌ای
        /// </summary>
        public async Task<string> GenerateQrCodeAsync(string secret, string email, string issuer = "Authorization Login")
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("کلید مخفی نمی‌تواند خالی باشد", nameof(secret));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            var uri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
            return await Task.FromResult(uri);
        }

        /// <summary>
        /// تولید کدهای پشتیبان برای احراز هویت دو مرحله‌ای
        /// </summary>
        public async Task<IEnumerable<string>> GenerateBackupCodesAsync(int count = 8)
        {
            if (count < 1 || count > 20)
                throw new ArgumentException("تعداد کدهای پشتیبان باید بین 1 تا 20 باشد", nameof(count));

            var codes = new List<string>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                var code = new char[8];
                for (int j = 0; j < code.Length; j++)
                {
                    code[j] = _backupCodeChars[random.Next(_backupCodeChars.Length)];
                }
                codes.Add(new string(code));
            }

            return await Task.FromResult(codes);
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly char[] _backupCodeChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    }
} 