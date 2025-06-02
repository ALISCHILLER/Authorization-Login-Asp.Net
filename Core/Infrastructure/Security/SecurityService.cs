using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    /// <summary>
    /// سرویس یکپارچه امنیتی
    /// این سرویس تمام عملیات مربوط به احراز هویت دو مرحله‌ای و مدیریت رمز عبور را در یک جا متمرکز می‌کند
    /// </summary>
    public class SecurityService : ITwoFactorService, IPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;
        private readonly ITracingService _tracingService;
        private readonly SecurityOptions _securityOptions;

        public SecurityService(
            IUserRepository userRepository,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
            ITracingService tracingService,
            IOptions<SecurityOptions> securityOptions)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _securityOptions = securityOptions?.Value ?? throw new ArgumentNullException(nameof(securityOptions));
        }

        #region ITwoFactorService Implementation

        public async Task<bool> EnableTwoFactorAsync(Guid userId, TwoFactorType type)
        {
            using var activity = _tracingService.StartActivity("SecurityService.EnableTwoFactorAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای قبلاً فعال شده است");

                // تولید کد تأیید
                var verificationCode = GenerateVerificationCode();
                var expiryTime = DateTime.UtcNow.AddMinutes(_securityOptions.TwoFactorCodeExpiryMinutes);

                // ذخیره کد تأیید
                user.SetTwoFactorVerification(verificationCode, expiryTime, type);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال کد تأیید
                switch (type)
                {
                    case TwoFactorType.Email:
                        await _emailService.SendTwoFactorCodeAsync(user.Email.Value, verificationCode);
                        break;
                    case TwoFactorType.Sms:
                        await _smsService.SendTwoFactorCodeAsync(user.PhoneNumber, verificationCode);
                        break;
                    default:
                        throw new DomainException("نوع احراز هویت دو مرحله‌ای نامعتبر است");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در فعال‌سازی احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<bool> DisableTwoFactorAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("SecurityService.DisableTwoFactorAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");

                if (!user.VerifyTwoFactorCode(code))
                    throw new DomainException("کد تأیید نامعتبر است");

                user.DisableTwoFactor();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<bool> ValidateTwoFactorAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ValidateTwoFactorAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");

                if (!user.VerifyTwoFactorCode(code))
                    throw new DomainException("کد تأیید نامعتبر است");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید کد احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<bool> ResendTwoFactorCodeAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ResendTwoFactorCodeAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");

                // تولید کد تأیید جدید
                var verificationCode = GenerateVerificationCode();
                var expiryTime = DateTime.UtcNow.AddMinutes(_securityOptions.TwoFactorCodeExpiryMinutes);

                // ذخیره کد تأیید جدید
                user.SetTwoFactorVerification(verificationCode, expiryTime, user.TwoFactorType);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال کد تأیید جدید
                switch (user.TwoFactorType)
                {
                    case TwoFactorType.Email:
                        await _emailService.SendTwoFactorCodeAsync(user.Email.Value, verificationCode);
                        break;
                    case TwoFactorType.Sms:
                        await _smsService.SendTwoFactorCodeAsync(user.PhoneNumber, verificationCode);
                        break;
                    default:
                        throw new DomainException("نوع احراز هویت دو مرحله‌ای نامعتبر است");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال مجدد کد احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        #endregion

        #region IPasswordService Implementation

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ChangePasswordAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.VerifyPassword(currentPassword))
                    throw new DomainException("رمز عبور فعلی اشتباه است");

                user.ChangePassword(newPassword);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال اعلان تغییر رمز عبور
                await _emailService.SendPasswordChangedNotificationAsync(user.Email.Value);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تغییر رمز عبور");
                throw;
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ResetPasswordAsync");
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                // تولید کد تأیید
                var resetCode = GenerateVerificationCode();
                var expiryTime = DateTime.UtcNow.AddMinutes(_securityOptions.PasswordResetCodeExpiryMinutes);

                // ذخیره کد تأیید
                user.SetPasswordResetCode(resetCode, expiryTime);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال کد تأیید
                await _emailService.SendPasswordResetCodeAsync(email, resetCode);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در درخواست بازنشانی رمز عبور");
                throw;
            }
        }

        public async Task<bool> ConfirmPasswordResetAsync(string email, string code, string newPassword)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ConfirmPasswordResetAsync");
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.VerifyPasswordResetCode(code))
                    throw new DomainException("کد تأیید نامعتبر است");

                user.ChangePassword(newPassword);
                user.ClearPasswordResetCode();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال اعلان بازنشانی رمز عبور
                await _emailService.SendPasswordResetNotificationAsync(email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید بازنشانی رمز عبور");
                throw;
            }
        }

        public async Task<bool> ValidatePasswordStrengthAsync(string password)
        {
            using var activity = _tracingService.StartActivity("SecurityService.ValidatePasswordStrengthAsync");
            try
            {
                // بررسی طول رمز عبور
                if (password.Length < _securityOptions.MinimumPasswordLength)
                    return false;

                // بررسی پیچیدگی رمز عبور
                var hasUpperCase = false;
                var hasLowerCase = false;
                var hasDigit = false;
                var hasSpecialChar = false;

                foreach (var c in password)
                {
                    if (char.IsUpper(c)) hasUpperCase = true;
                    else if (char.IsLower(c)) hasLowerCase = true;
                    else if (char.IsDigit(c)) hasDigit = true;
                    else if (!char.IsLetterOrDigit(c)) hasSpecialChar = true;
                }

                return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی قدرت رمز عبور");
                throw;
            }
        }

        #endregion

        #region Private Methods

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        #endregion
    }
} 