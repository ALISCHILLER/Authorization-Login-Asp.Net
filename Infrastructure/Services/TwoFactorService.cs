using System;
using System.Security.Cryptography;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Enums;
using Microsoft.Extensions.Logging;
using OtpNet;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ILogger<TwoFactorService> _logger;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public TwoFactorService(
            ILogger<TwoFactorService> logger,
            IEmailService emailService,
            ISmsService smsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        }

        public string GenerateSecret()
        {
            try
            {
                var key = KeyGeneration.GenerateRandomKey(20);
                return Base32Encoding.ToString(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating 2FA secret");
                throw;
            }
        }

        public bool ValidateToken(string secret, string token)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secret));
                return totp.VerifyTotp(token, out _, new VerificationWindow(2, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating 2FA token");
                throw;
            }
        }

        public string GenerateQrCodeUri(string secret, string email, string issuer)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(issuer))
                throw new ArgumentNullException(nameof(issuer));

            try
            {
                return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code URI");
                throw;
            }
        }

        public string GenerateRecoveryCode()
        {
            try
            {
                using var rng = RandomNumberGenerator.Create();
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recovery code");
                throw;
            }
        }

        public async Task<bool> EnableAsync(User user, TwoFactorType type)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var secret = GenerateSecret();
                user.EnableTwoFactor(type, secret);

                // Generate and add recovery codes
                for (int i = 0; i < 10; i++)
                {
                    var code = GenerateRecoveryCode();
                    user.AddRecoveryCode(code, DateTime.UtcNow.AddDays(30));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> DisableAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                user.DisableTwoFactor();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling 2FA for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> SendCodeAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
                var code = totp.ComputeTotp();

                switch (user.TwoFactorType)
                {
                    case TwoFactorType.Email:
                        await _emailService.SendTwoFactorCodeAsync(user.Email.Value, code);
                        break;
                    case TwoFactorType.Sms:
                        await _smsService.SendTwoFactorCodeAsync(user.PhoneNumber, code);
                        break;
                    default:
                        throw new ArgumentException("Unsupported two-factor authentication type");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending 2FA code to user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> VerifyCodeAsync(User user, string code)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));

            try
            {
                // First check if it's a recovery code
                if (user.ValidateRecoveryCode(code))
                {
                    return true;
                }

                // If not a recovery code, validate as TOTP
                return ValidateToken(user.TwoFactorSecret, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying 2FA code for user {UserId}", user.Id);
                return false;
            }
        }
    }
} 