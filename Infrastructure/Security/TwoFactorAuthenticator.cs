using System;
using System.Security.Cryptography;
using System.Text;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Infrastructure.Security
{
    /// <summary>
    /// کلاس مدیریت احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorAuthenticator : ITwoFactorAuthenticator
    {
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<TwoFactorAuthenticator> _logger;
        private const int RECOVERY_CODE_LENGTH = 10;
        private const int RECOVERY_CODE_COUNT = 10;
        private const int CODE_VALIDITY_MINUTES = 5;

        public TwoFactorAuthenticator(
            IEmailService emailService, 
            ISmsService smsService,
            ILogger<TwoFactorAuthenticator> logger)
        {
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        /// <summary>
        /// تولید کلید مخفی برای احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>کلید مخفی</returns>
        public string GenerateSecretKey()
        {
            try
            {
                var key = KeyGeneration.GenerateRandomKey(20);
                return Base32Encoding.ToString(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating secret key for 2FA");
                throw new TwoFactorAuthenticationException("Failed to generate secret key", ex);
            }
        }

        /// <summary>
        /// تولید کد یکبار مصرف
        /// </summary>
        /// <param name="secretKey">کلید مخفی</param>
        /// <returns>کد یکبار مصرف</returns>
        public string GenerateCode(string secretKey)
        {
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                return totp.ComputeTotp();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating 2FA code");
                throw new TwoFactorAuthenticationException("Failed to generate code", ex);
            }
        }

        /// <summary>
        /// بررسی اعتبار کد یکبار مصرف
        /// </summary>
        /// <param name="secretKey">کلید مخفی</param>
        /// <param name="code">کد یکبار مصرف</param>
        /// <returns>نتیجه بررسی</returns>
        public bool ValidateCode(string secretKey, string code)
        {
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating 2FA code");
                throw new TwoFactorAuthenticationException("Failed to validate code", ex);
            }
        }

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        /// <param name="count">تعداد کدها</param>
        /// <returns>کدهای بازیابی</returns>
        public string[] GenerateRecoveryCodes(int count = RECOVERY_CODE_COUNT)
        {
            try
            {
                var codes = new string[count];
                for (int i = 0; i < count; i++)
                {
                    codes[i] = GenerateRecoveryCode();
                }
                return codes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recovery codes");
                throw new TwoFactorAuthenticationException("Failed to generate recovery codes", ex);
            }
        }

        /// <summary>
        /// ارسال کد یکبار مصرف
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="code">کد یکبار مصرف</param>
        public async Task SendCodeAsync(User user, string code)
        {
            try
            {
                switch (user.TwoFactorType)
                {
                    case TwoFactorType.Email:
                        await _emailService.SendTwoFactorCodeAsync(user.Email.Value, code);
                        break;
                    case TwoFactorType.Sms:
                        await _smsService.SendTwoFactorCodeAsync(user.PhoneNumber, code);
                        break;
                    default:
                        throw new TwoFactorAuthenticationException("Invalid two-factor authentication type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending 2FA code to user {UserId}", user.Id);
                throw new TwoFactorAuthenticationException("Failed to send code", ex);
            }
        }

        /// <summary>
        /// تولید QR Code برای اسکن با Google Authenticator
        /// </summary>
        /// <param name="secretKey">کلید مخفی</param>
        /// <param name="email">ایمیل کاربر</param>
        /// <returns>QR Code به صورت Base64</returns>
        public string GenerateQrCode(string secretKey, string email)
        {
            try
            {
                var issuer = "YourAppName";
                var accountTitle = email;
                var provisioningUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountTitle)}?secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new BitmapByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                return Convert.ToBase64String(qrCodeImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code for 2FA");
                throw new TwoFactorAuthenticationException("Failed to generate QR code", ex);
            }
        }

        /// <summary>
        /// بررسی اعتبار کد بازیابی
        /// </summary>
        /// <param name="recoveryCode">کد بازیابی</param>
        /// <param name="storedCodes">کدهای بازیابی ذخیره شده</param>
        /// <returns>نتیجه بررسی</returns>
        public bool ValidateRecoveryCode(string recoveryCode, IEnumerable<string> storedCodes)
        {
            try
            {
                return storedCodes.Contains(recoveryCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating recovery code");
                throw new TwoFactorAuthenticationException("Failed to validate recovery code", ex);
            }
        }

        /// <summary>
        /// تولید کد بازیابی
        /// </summary>
        /// <returns>کد بازیابی</returns>
        private string GenerateRecoveryCode()
        {
            var bytes = new byte[RECOVERY_CODE_LENGTH];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Substring(0, RECOVERY_CODE_LENGTH);
        }
    }

    public class TwoFactorAuthenticationException : Exception
    {
        public TwoFactorAuthenticationException(string message) : base(message) { }
        public TwoFactorAuthenticationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 