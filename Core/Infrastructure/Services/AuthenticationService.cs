using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using OtpNet;
using QRCoder;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories;
using AutoMapper;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// سرویس یکپارچه احراز هویت و مدیریت ورود کاربران
    /// این سرویس تمام عملیات مربوط به احراز هویت، ورود و خروج، تاریخچه ورود و امنیت را در یک جا متمرکز می‌کند
    /// </summary>
    public class AuthenticationService : IUserService, ILoginHistoryService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IImageService _imageService;
        private readonly ITracingService _tracingService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<AuthenticationService> logger,
            IImageService imageService,
            ITracingService tracingService,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        // ثبت‌نام کاربر جدید
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.ExistsByUsernameAsync(request.Username))
                throw new DomainException("نام کاربری قبلاً استفاده شده است");
            if (await _userRepository.ExistsByEmailAsync(request.Email))
                throw new DomainException("ایمیل قبلاً استفاده شده است");
            var user = User.Create(
                request.Username,
                request.Email,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                request.Password);
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            await _emailService.SendVerificationEmailAsync(user.Email.Value, user.Id);
            var token = await _jwtService.GenerateTokenAsync(user);
            return new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        // ورود کاربر
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !user.VerifyPassword(request.Password))
            {
                if (user != null)
                {
                    user.IncrementFailedLoginAttempts();
                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();
                }
                throw new DomainException("نام کاربری یا رمز عبور اشتباه است");
            }
            if (user.IsAccountLocked())
                throw new DomainException("حساب کاربری شما قفل شده است");
            user.LastLoginAt = DateTime.UtcNow;
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            var token = await _jwtService.GenerateTokenAsync(user);
            return new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                User = _mapper.Map<UserDto>(user),
                RequiresTwoFactor = user.TwoFactorEnabled
            };
        }

        // ثبت تاریخچه ورود موفق
        public async Task<LoginHistory> LogSuccessfulLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceName = deviceInfo.DeviceName,
                DeviceType = deviceInfo.DeviceType,
                OperatingSystem = deviceInfo.OperatingSystem,
                Browser = deviceInfo.BrowserName,
                IsSuccessful = true
            };
            await _userRepository.AddLoginHistoryAsync(loginHistory);
            await _userRepository.SaveChangesAsync();
            return loginHistory;
        }

        // ثبت تاریخچه ورود ناموفق
        public async Task<LoginHistory> LogFailedLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo, string failureReason)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceName = deviceInfo.DeviceName,
                DeviceType = deviceInfo.DeviceType,
                OperatingSystem = deviceInfo.OperatingSystem,
                Browser = deviceInfo.BrowserName,
                IsSuccessful = false,
                FailureReason = failureReason
            };
            await _userRepository.AddLoginHistoryAsync(loginHistory);
            await _userRepository.SaveChangesAsync();
            return loginHistory;
        }

        // ثبت خروج کاربر
        public async Task LogLogoutAsync(Guid userId)
        {
            var lastLogin = await _userRepository.GetLastLoginHistoryAsync(userId);
            if (lastLogin != null && !lastLogin.LogoutTime.HasValue)
            {
                lastLogin.LogoutTime = DateTime.UtcNow;
                lastLogin.SessionDuration = (int)(lastLogin.LogoutTime.Value - lastLogin.LoginTime).TotalSeconds;
                await _userRepository.UpdateLoginHistoryAsync(lastLogin);
                await _userRepository.SaveChangesAsync();
            }
        }

        // دریافت تاریخچه ورود کاربر
        public async Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var items = await _userRepository.GetLoginHistoryAsync(userId, page, pageSize);
            var totalCount = await _userRepository.GetLoginHistoryCountAsync(userId);
            return (items.ToList(), totalCount);
        }

        // دریافت آخرین ورود موفق
        public async Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId)
        {
            return await _userRepository.GetLastSuccessfulLoginAsync(userId);
        }

        // تعداد تلاش ناموفق
        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15)
        {
            return await _userRepository.GetFailedLoginAttemptsCountAsync(userId, timeWindowMinutes);
        }

        // Two-Factor Authentication
        public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (user.TwoFactorEnabled)
                throw new DomainException("احراز هویت دو مرحله‌ای قبلاً فعال شده است");
            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key);
            var issuer = _configuration["Authentication:TwoFactor:Issuer"] ?? "Authorization Login";
            var accountTitle = user.Email.Value;
            var provisioningUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountTitle)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrCodeData);
            using var qrCodeImage = qrCode.GetGraphic(20);
            user.TwoFactorSecret = secret;
            user.TwoFactorEnabled = false;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            return new TwoFactorSetupResponse
            {
                Secret = secret,
                QrCodeImage = qrCodeImage,
                ManualEntryKey = secret
            };
        }

        public async Task<bool> VerifyTwoFactorSetupAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new DomainException("کلید مخفی احراز هویت دو مرحله‌ای یافت نشد");
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
            if (isValid)
            {
                user.TwoFactorEnabled = true;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            return isValid;
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
        }

        public async Task DisableTwoFactorAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (!user.TwoFactorEnabled)
                throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");
            if (!await VerifyTwoFactorCodeAsync(userId, code))
                throw new DomainException("کد تأیید نامعتبر است");
            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<string> GenerateBackupCodesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (!user.TwoFactorEnabled)
                throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");
            var backupCodes = new List<string>();
            var random = new Random();
            for (int i = 0; i < 8; i++)
                backupCodes.Add(random.Next(10000000, 99999999).ToString());
            user.BackupCodes = backupCodes.Select(code => BCrypt.Net.BCrypt.HashPassword(code)).ToList();
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();
            await _emailService.SendBackupCodesAsync(user.Email.Value, backupCodes);
            return string.Join("\n", backupCodes);
        }

        public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("کاربر یافت نشد");
            if (!user.TwoFactorEnabled || user.BackupCodes == null || !user.BackupCodes.Any())
                throw new DomainException("کد پشتیبان موجود نیست");
            var isValid = user.BackupCodes.Any(hash => BCrypt.Net.BCrypt.Verify(code, hash));
            if (isValid)
            {
                user.BackupCodes.RemoveAll(hash => BCrypt.Net.BCrypt.Verify(code, hash));
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }
            return isValid;
        }
    }
}