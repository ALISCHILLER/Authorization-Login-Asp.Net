using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet; // Used in AuthenticationService
using QRCoder; // Used in AuthenticationService
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Common; // For DomainException (if needed)
using AutoMapper; // For mapping User to UserDto in ValidateTwoFactorLoginAsync

// Changed namespace to match expected location
namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TwoFactorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IPasswordHasher _passwordHasher; // For hashing backup codes
        private readonly IJwtService _jwtService; // For generating tokens after 2FA validation
        private readonly IMapper _mapper; // For mapping User to UserDto
        private readonly IDateTimeService _dateTimeService; // Injected


        public TwoFactorService(
            IUserRepository userRepository,
            ILogger<TwoFactorService> logger,
            IConfiguration configuration,
            IEmailService emailService,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IMapper mapper,
            IDateTimeService dateTimeService) // Added
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService)); // Added
        }

        public async Task<TwoFactorSetupResponse> SetupTwoFactorAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new DomainException("User not found."); // Translated, Consider NotFoundException
            if (user.TwoFactorEnabled)
                throw new DomainException("Two-factor authentication is already enabled."); // Translated, Consider ConflictException

            var key = KeyGeneration.GenerateRandomKey(20); // From AuthenticationService
            var secret = Base32Encoding.ToString(key);
            var issuer = _configuration["Authentication:TwoFactor:Issuer"] ?? _configuration["JwtSettings:Issuer"] ?? "YourAppName";
            var accountTitle = user.EmailAddress; // Assuming EmailAddress is string, if it's an Email VO, use .Value

            var provisioningUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountTitle)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";

            // Generate QR Code
            // byte[] qrCodeImageBytes; // If you need to return bytes
            string qrCodeImageUrl; // Or return as data URL
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.Q))
            // If returning image as base64 string:
            using (var qrCode = new PngByteQRCode(qrCodeData)) // Or BitmapByteQRCode for other formats
            {
                byte[] qrCodeImageBytes = qrCode.GetGraphic(20);
                qrCodeImageUrl = $"data:image/png;base64,{Convert.ToBase64String(qrCodeImageBytes)}";
            }
            // else if returning raw bytes:
            // using (var qrCode = new QRCode(qrCodeData))
            // using (var qrCodeImage = qrCode.GetGraphic(20)) // This returns a Bitmap, might not be ideal for API.
            // {
            //    // Convert Bitmap to byte array or use a library that directly outputs byte[] for PNG/JPEG
            // }


            user.TwoFactorSecret = secret;
            // user.TwoFactorEnabled = false; // User is not yet fully enabled, only after verification.
            await _userRepository.UpdateAsync(user);
            // await _userRepository.SaveChangesAsync(); // Assuming UoW handles this

            _logger.LogInformation("2FA setup initiated for User ID: {UserId}", userId);
            return new TwoFactorSetupResponse
            {
                Secret = secret, // Manual Entry Key
                QrCodeImageUrl = qrCodeImageUrl, // For displaying QR code
                ManualEntryKey = secret // Same as Secret, for clarity
            };
        }

        public async Task<bool> VerifyTwoFactorSetupAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated
            if (string.IsNullOrEmpty(user.TwoFactorSecret)) throw new DomainException("2FA secret key not found."); // Translated

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(2, 2)); // 2 past, 2 future codes (1 min window)

            if (isValid)
            {
                user.TwoFactorEnabled = true;
                await _userRepository.UpdateAsync(user);
                // await _userRepository.SaveChangesAsync();
                _logger.LogInformation("2FA setup verified and enabled for User ID: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Invalid 2FA setup verification code for User ID: {UserId}", userId);
            }
            return isValid;
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                _logger.LogWarning("2FA code verification attempt for User ID: {UserId} where 2FA is not enabled or secret is missing.", userId);
                // Consider throwing DomainException("Two-factor authentication is not enabled.") for clarity
                return false;
            }

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            bool isValid = totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
            if (!isValid)
            {
                _logger.LogWarning("Invalid 2FA code for User ID: {UserId}", userId);
            }
            return isValid;
        }

        public async Task<AuthResponse> ValidateTwoFactorLoginAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated

            if (!user.TwoFactorEnabled)
                throw new DomainException("2FA is not enabled for this user."); // Already English

            if (!await VerifyTwoFactorCodeAsync(userId, code))
                throw new DomainException("Invalid 2FA code."); // Translated

            // If code is valid, generate new tokens
            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()); // Assuming IHttpContextAccessor is available or IP is passed

            _logger.LogInformation("2FA login validated for User ID: {UserId}", userId);
            return new AuthResponse
            {
                IsSuccess = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token, // Assuming RefreshToken object has a Token string property
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task DisableTwoFactorAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated
            if (!user.TwoFactorEnabled) throw new DomainException("Two-factor authentication is not enabled."); // Translated

            if (!await VerifyTwoFactorCodeAsync(userId, code)) // Verify with current TOTP code
                throw new DomainException("Invalid verification code."); // Translated

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.BackupCodes = new List<string>(); // Clear backup codes as well
            await _userRepository.UpdateAsync(user);
            // await _userRepository.SaveChangesAsync();
            _logger.LogInformation("2FA disabled for User ID: {UserId}", userId);
        }

        public async Task<IEnumerable<string>> GenerateAndStoreBackupCodesAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated
            if (!user.TwoFactorEnabled) throw new DomainException("Two-factor authentication must be enabled to generate backup codes."); // Translated

            var backupCodesPlain = new List<string>();
            var backupCodesHashed = new List<string>();
            var random = new Random();

            for (int i = 0; i < 8; i++) // Generate 8 codes
            {
                string plainCode = random.Next(10000000, 99999999).ToString("D8"); // 8-digit codes
                backupCodesPlain.Add(plainCode);
                // Hash the backup code before storing
                var (hash, salt) = await _passwordHasher.HashPasswordAsync(plainCode); // Assuming IPasswordHasher can be used
                backupCodesHashed.Add($"{hash}:{salt}"); // Store hash and salt
            }

            user.BackupCodes = backupCodesHashed;
            await _userRepository.UpdateAsync(user);
            // await _userRepository.SaveChangesAsync();

            // Send backup codes via email
            await _emailService.SendBackupCodesAsync(user.EmailAddress, backupCodesPlain); // Assuming user.EmailAddress is the correct string property for email
            _logger.LogInformation("Generated and sent backup codes for User ID: {UserId}", userId);
            return backupCodesPlain;
        }

        public async Task<bool> VerifyAndConsumeBackupCodeAsync(Guid userId, string code)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new DomainException("User not found."); // Translated
            if (user.BackupCodes == null || !user.BackupCodes.Any())
            {
                _logger.LogWarning("No backup codes found for User ID: {UserId} during verification.", userId);
                return false;
            }

            string? foundHashedCode = null;
            foreach (var storedHashEntry in user.BackupCodes)
            {
                var parts = storedHashEntry.Split(':');
                if (parts.Length == 2)
                {
                    var storedHash = parts[0];
                    var storedSalt = parts[1];
                    if (await _passwordHasher.VerifyPasswordAsync(code, storedHash, storedSalt))
                    {
                        foundHashedCode = storedHashEntry;
                        break;
                    }
                }
            }

            if (foundHashedCode != null)
            {
                user.BackupCodes.Remove(foundHashedCode); // Consume the code
                await _userRepository.UpdateAsync(user);
                // await _userRepository.SaveChangesAsync();
                _logger.LogInformation("Backup code verified and consumed for User ID: {UserId}", userId);
                return true;
            }

            _logger.LogWarning("Invalid backup code provided for User ID: {UserId}", userId);
            return false;
        }

        public async Task<bool> IsTwoFactorEnabledAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("IsTwoFactorEnabledAsync check for non-existent User ID: {UserId}", userId);
                return false; // Or throw NotFoundException
            }
            return user.TwoFactorEnabled;
        }
    }
}