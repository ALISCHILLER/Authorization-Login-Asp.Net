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
using Authorization_Login_Asp.Net.Core.Application.Interfaces; // For IPasswordHasher
using Authorization_Login_Asp.Net.Core.Application.Interfaces.Services; // For IUserAuthenticationService
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth; // For AuthResponse, RegisterRequest, LoginRequest etc.

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// سرویس یکپارچه احراز هویت و مدیریت ورود کاربران
    /// این سرویس تمام عملیات مربوط به احراز هویت، ورود و خروج، تاریخچه ورود و امنیت را در یک جا متمرکز می‌کند
    /// </summary>
    public class AuthenticationService : IUserService, ILoginHistoryService, IUserAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher; // Added
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
            IPasswordHasher passwordHasher, // Added
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
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher)); // Added
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
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
                throw new DomainException("نام کاربری قبلاً استفاده شده است");
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
                throw new DomainException("ایمیل قبلاً استفاده شده است");

            var (hash, salt) = await _passwordHasher.HashPasswordAsync(request.Password);

            var user = new User( // Assuming User constructor takes all necessary fields or properties are settable
                username: request.Username,
                email: request.Email,
                passwordHash: hash, // Pass the generated hash
                firstName: request.FirstName,
                lastName: request.LastName,
                phoneNumber: request.PhoneNumber
            );
            user.PasswordSalt = salt; // Set the generated salt

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync(cancellationToken);
            // await _emailService.SendVerificationEmailAsync(user.Email.Value, user.Id.ToString()); // user.Id is Guid, link might need string
            var token = await _jwtService.GenerateAccessTokenAsync(user); // Assuming GenerateAccessTokenAsync exists
            return new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        // ورود کاربر
        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            // Try fetching user by username or email.
            // This logic might need to be more sophisticated if username and email are not distinct or if one is preferred.
            var user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(request.UsernameOrEmail, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login attempt for non-existent user: {UsernameOrEmail}", request.UsernameOrEmail);
                throw new DomainException("نام کاربری یا رمز عبور اشتباه است");
            }

            bool passwordVerified = await _passwordHasher.VerifyPasswordAsync(request.Password, user.PasswordHash, user.PasswordSalt);

            if (!passwordVerified)
            {
                user.IncrementFailedLoginAttempts();
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Failed login attempt for user: {UsernameOrEmail}", request.UsernameOrEmail);
                throw new DomainException("نام کاربری یا رمز عبور اشتباه است");
            }

            if (user.IsAccountLocked())
            {
                _logger.LogWarning("Login attempt for locked account: {UsernameOrEmail}", request.UsernameOrEmail);
                throw new DomainException($"حساب کاربری شما تا {user.AccountLockoutEnd} قفل شده است");
            }

            user.LastLoginAt = DateTime.UtcNow; // This should ideally come from a DateTime service
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync(cancellationToken);

            var token = await _jwtService.GenerateAccessTokenAsync(user); // Assuming GenerateAccessTokenAsync
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

            // Assuming backup codes are stored hashed. This logic needs to be robust.
            // For simplicity, let's assume BCrypt was used for backup codes as per original code.
            // This should ideally also use IPasswordHasher if backup codes are to be treated like passwords.
            var validBackupCodeHash = user.BackupCodes.FirstOrDefault(hash => BCrypt.Net.BCrypt.Verify(code, hash));
            if (validBackupCodeHash != null)
            {
                user.BackupCodes.Remove(validBackupCodeHash);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Implementation for IUserAuthenticationService methods
        public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(usernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(usernameOrEmail, cancellationToken);

            if (user == null) return null;

            bool passwordVerified = await _passwordHasher.VerifyPasswordAsync(password, user.PasswordHash, user.PasswordSalt);
            return passwordVerified ? user : null;
        }

        public async Task<AuthResponse> ValidateTwoFactorAsync(TwoFactorRequest request, CancellationToken cancellationToken = default)
        {
            // TODO: Implement actual 2FA validation logic, this is a placeholder
            _logger.LogInformation("Validating 2FA for User ID: {UserId}", request.UserId);
            var user = await _userRepository.GetByIdAsync(Guid.Parse(request.UserId), cancellationToken);
            if (user == null) throw new DomainException("کاربر یافت نشد");

            // This is where Otp.NET or similar should be used with user.TwoFactorSecret
            // For now, assuming it's successful if user has 2FA enabled.
            if (!user.TwoFactorEnabled) throw new DomainException("2FA is not enabled for this user.");

            bool isValidCode = await this.VerifyTwoFactorCodeAsync(user.Id, request.Code); // Using existing method for now
            if(!isValidCode) throw new DomainException("کد تایید دو مرحله ای نامعتبر است.");

            var token = await _jwtService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user); // IP address might be needed here from request

            return new AuthResponse { IsSuccess = true, Token = token, RefreshToken = refreshToken, User = _mapper.Map<UserDto>(user) };
        }

        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            // TODO: Implement refresh token logic
            _logger.LogInformation("Refreshing token: {RefreshToken}", request.Token);
            throw new NotImplementedException();
        }

        public Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            // TODO: Implement token revocation
            _logger.LogInformation("Revoking token: {Token} from IP: {IpAddress}", token, ipAddress);
            throw new NotImplementedException();
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            // TODO: Implement more robust token validation if needed beyond JwtService
            _logger.LogInformation("Validating token: {Token}", token);
            return Task.FromResult(_jwtService.ValidateToken(token));
        }

        public async Task<User> GetUserFromTokenAsync(string token)
        {
            // TODO: Implement user retrieval from token claims
            _logger.LogInformation("Getting user from token: {Token}", token);
            var principal = _jwtService.GetPrincipalFromToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return await _userRepository.GetByIdAsync(userId);
            }
            return null;
        }

        public async Task<bool> IsEmailConfirmedAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.IsEmailVerified ?? false;
        }

        public async Task<bool> IsPhoneNumberConfirmedAsync(string phoneNumber)
        {
            // Assuming a method like GetByPhoneNumberAsync exists or is added to IUserRepository
            // var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            // return user?.IsPhoneVerified ?? false;
            _logger.LogWarning("IsPhoneNumberConfirmedAsync is not fully implemented.");
            return await Task.FromResult(false); // Placeholder
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt))
                return false;
            return await _passwordHasher.VerifyPasswordAsync(password, user.PasswordHash, user.PasswordSalt);
        }

        public Task<bool> IsLockedOutAsync(User user)
        {
            return Task.FromResult(user?.IsAccountLocked() ?? false);
        }

        public async Task<int> GetRecentFailedAttemptsAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(usernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(usernameOrEmail, cancellationToken);
            return user?.FailedLoginAttempts ?? 0;
        }

        public async Task<DateTime?> GetAccountLockoutEndAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            return user?.AccountLockoutEnd;
        }

        public async Task RecordLoginAsync(Guid userId, string ipAddress, string? deviceToken, CancellationToken cancellationToken = default)
        {
            // This method was called by LoginCommandHandler. The actual logging is done via LogSuccessfulLoginAsync.
            // For now, this can be a pass-through or ensure LogSuccessfulLoginAsync is called appropriately.
            // DeviceInfo might need to be constructed or passed differently.
            _logger.LogInformation("Recording login for User ID: {UserId}, IP: {IpAddress}, DeviceToken: {DeviceToken}", userId, ipAddress, deviceToken);
            // Example: await LogSuccessfulLoginAsync(userId, ipAddress, deviceToken ?? "Unknown", new DeviceInfo { /* populate if possible */ });
            await Task.CompletedTask; // Placeholder, actual logging should happen
        }

        // IUserService implementation methods (GetByIdAsync, GetByEmailAsync, GetByUsernameAsync)
        public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return _mapper.Map<UserDto>(user);
        }
    }
}