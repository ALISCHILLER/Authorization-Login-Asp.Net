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
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
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
        private readonly ILoggingService _logger;
        private readonly IImageService _imageService;
        private readonly ITracingService _tracingService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthenticationService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
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

        #region IUserService Implementation

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.RegisterAsync");
            try
            {
                // بررسی تکراری نبودن نام کاربری و ایمیل
                if (await _userRepository.ExistsByUsernameAsync(request.Username))
                    throw new DomainException("نام کاربری قبلاً استفاده شده است");

                if (await _userRepository.ExistsByEmailAsync(request.Email))
                    throw new DomainException("ایمیل قبلاً استفاده شده است");

                // ایجاد کاربر جدید
                var user = User.Create(
                    request.Username,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber,
                    request.Password);

                // ذخیره کاربر
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال ایمیل تأیید
                await _emailService.SendVerificationEmailAsync(user.Email.Value, user.Id);

                // تولید توکن
                var token = await _jwtService.GenerateTokenAsync(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Token = token,
                    User = _mapper.Map<UserDto>(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت‌نام کاربر");
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.LoginAsync");
            try
            {
                var user = await _userRepository.GetByUsernameAsync(request.Username);
                if (user == null)
                    throw new DomainException("نام کاربری یا رمز عبور اشتباه است");

                if (!user.VerifyPassword(request.Password))
                {
                    user.IncrementFailedLoginAttempts();
                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();
                    throw new DomainException("نام کاربری یا رمز عبور اشتباه است");
                }

                if (user.IsAccountLocked())
                    throw new DomainException("حساب کاربری شما قفل شده است");

                // ثبت ورود موفق
                var loginHistory = await LogSuccessfulLoginAsync(
                    user.Id,
                    request.IpAddress,
                    request.UserAgent,
                    new DeviceInfo
                    {
                        DeviceName = request.DeviceName,
                        DeviceType = request.DeviceType,
                        OperatingSystem = request.OperatingSystem,
                        BrowserName = request.BrowserName
                    });

                // به‌روزرسانی آخرین ورود
                user.LastLoginAt = DateTime.UtcNow;
                user.ResetFailedLoginAttempts();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // تولید توکن
                var token = await _jwtService.GenerateTokenAsync(user);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Token = token,
                    User = _mapper.Map<UserDto>(user),
                    RequiresTwoFactor = user.TwoFactorEnabled
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ورود کاربر");
                throw;
            }
        }

        // ... سایر متدهای IUserService

        #endregion

        #region ILoginHistoryService Implementation

        public async Task<LoginHistory> LogSuccessfulLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.LogSuccessfulLoginAsync");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت ورود موفق");
                throw;
            }
        }

        public async Task<LoginHistory> LogFailedLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo, string failureReason)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.LogFailedLoginAsync");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت ورود ناموفق");
                throw;
            }
        }

        public async Task LogLogoutAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.LogLogoutAsync");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت خروج");
                throw;
            }
        }

        public async Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.GetUserLoginHistoryAsync");
            try
            {
                var items = await _userRepository.GetLoginHistoryAsync(userId, page, pageSize);
                var totalCount = await _userRepository.GetLoginHistoryCountAsync(userId);
                return (items.ToList(), totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود");
                throw;
            }
        }

        public async Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.GetLastSuccessfulLoginAsync");
            try
            {
                return await _userRepository.GetLastSuccessfulLoginAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آخرین ورود موفق");
                throw;
            }
        }

        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15)
        {
            using var activity = _tracingService.StartActivity("UserManagementService.GetFailedLoginAttemptsCountAsync");
            try
            {
                return await _userRepository.GetFailedLoginAttemptsCountAsync(userId, timeWindowMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد تلاش‌های ناموفق");
                throw;
            }
        }

        #endregion

        #region Two-Factor Authentication

        public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.SetupTwoFactorAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای قبلاً فعال شده است");

                // تولید کلید مخفی
                var key = KeyGeneration.GenerateRandomKey(20);
                var secret = Base32Encoding.ToString(key);

                // تولید QR کد
                var issuer = _configuration["Authentication:TwoFactor:Issuer"] ?? "Authorization Login";
                var accountTitle = $"{user.Email.Value}";
                var provisioningUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountTitle)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrCodeData);
                using var qrCodeImage = qrCode.GetGraphic(20);

                // ذخیره کلید مخفی موقت
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در راه‌اندازی احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<bool> VerifyTwoFactorSetupAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.VerifyTwoFactorSetupAsync");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید راه‌اندازی احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.VerifyTwoFactorCodeAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                    throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");

                var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
                return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید کد احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task DisableTwoFactorAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.DisableTwoFactorAsync");
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در غیرفعال کردن احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        public async Task<string> GenerateBackupCodesAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.GenerateBackupCodesAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled)
                    throw new DomainException("احراز هویت دو مرحله‌ای فعال نیست");

                // تولید 8 کد پشتیبان 8 رقمی
                var backupCodes = new List<string>();
                var random = new Random();
                for (int i = 0; i < 8; i++)
                {
                    var code = random.Next(10000000, 99999999).ToString();
                    backupCodes.Add(code);
                }

                // ذخیره کدهای پشتیبان هش شده
                user.BackupCodes = backupCodes.Select(code => BCrypt.Net.BCrypt.HashPassword(code)).ToList();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال کدهای پشتیبان به ایمیل کاربر
                await _emailService.SendBackupCodesAsync(user.Email.Value, backupCodes);

                return string.Join("\n", backupCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تولید کدهای پشتیبان");
                throw;
            }
        }

        public async Task<bool> VerifyBackupCodeAsync(Guid userId, string code)
        {
            using var activity = _tracingService.StartActivity("AuthenticationService.VerifyBackupCodeAsync");
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new DomainException("کاربر یافت نشد");

                if (!user.TwoFactorEnabled || user.BackupCodes == null || !user.BackupCodes.Any())
                    throw new DomainException("کد پشتیبان موجود نیست");

                // بررسی کد پشتیبان
                var isValid = user.BackupCodes.Any(hash => BCrypt.Net.BCrypt.Verify(code, hash));
                if (isValid)
                {
                    // حذف کد استفاده شده
                    user.BackupCodes.RemoveAll(hash => BCrypt.Net.BCrypt.Verify(code, hash));
                    await _userRepository.UpdateAsync(user);
                    await _userRepository.SaveChangesAsync();
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تأیید کد پشتیبان");
                throw;
            }
        }

        #endregion
    }
} 