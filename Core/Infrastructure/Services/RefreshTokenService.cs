using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس توکن رفرش
    /// شامل تمام منطق‌های مربوط به تولید، تمدید و باطل کردن توکن‌ها
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
        // تنظیمات و وابستگی‌های لازم برای عملکرد سرویس
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly int _tokenExpirationDays;
        private readonly int _maxActiveTokensPerUser;

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public RefreshTokenService(
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger,
            IRefreshTokenRepository refreshTokenRepository)
        {
            // اعتبارسنجی وابستگی‌ها
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));

            // خواندن تنظیمات از appsettings.json
            _tokenExpirationDays = _configuration.GetValue("AppSettings:JwtSettings:RefreshTokenExpiryDays", 7);
            _maxActiveTokensPerUser = _configuration.GetValue("AppSettings:JwtSettings:MaxActiveRefreshTokensPerUser", 5);
        }

        /// <inheritdoc/>
        /// <summary>
        /// تولید یک توکن رفرش جدید برای کاربر مشخص شده
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(Guid userId)
        {
            try
            {
                // بررسی تعداد توکن‌های فعال کاربر
                var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

                // اگر تعداد توکن‌های فعال بیشتر از حد مجاز است، همه را باطل کن
                if (activeTokens.Count >= _maxActiveTokensPerUser)
                {
                    foreach (var token in activeTokens)
                    {
                        await RevokeRefreshTokenAsync(token.Token);
                    }
                }

                // ایجاد یک توکن جدید
                var refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = GenerateRandomToken(), // تولید توکن تصادفی
                    ExpiresAt = DateTime.UtcNow.AddDays(_tokenExpirationDays), // تعیین تاریخ انقضا
                    CreatedAt = DateTime.UtcNow, // زمان ایجاد
                    RevokedAt = null // توکن غیرفعال نیست
                };

                // ذخیره توکن در دیتابیس
                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                // ثبت لاگ
                _logger.LogInformation("Generated new refresh token for user {UserId}", userId);

                // بازگشت مقدار توکن
                return refreshToken.Token;
            }
            catch (Exception ex)
            {
                // ثبت خطای رخ داده
                _logger.LogError(ex, "Failed to generate refresh token for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to generate refresh token", ex);
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// تمدید توکن رفرش
        /// </summary>
        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // یافتن توکن در دیتابیس
                var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (token == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                // اعتبارسنجی توکن
                if (!token.IsValid())
                {
                    throw new RefreshTokenServiceException("Refresh token is no longer valid");
                }

                // تولید توکن جدید
                var newToken = await GenerateRefreshTokenAsync(token.UserId);

                // باطل کردن توکن قبلی
                token.Revoke("Rotated", token.Id);
                await _refreshTokenRepository.UpdateAsync(token);
                await _refreshTokenRepository.SaveChangesAsync();

                // ثبت لاگ
                _logger.LogInformation("Refreshed token for user {UserId}", token.UserId);

                // بازگشت توکن جدید
                return newToken;
            }
            catch (Exception ex)
            {
                // ثبت خطا
                _logger.LogError(ex, "Failed to refresh token");
                throw new RefreshTokenServiceException("Failed to refresh token", ex);
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// باطل کردن یک توکن رفرش مشخص شده
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                // یافتن توکن در دیتابیس
                var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (token == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                // باطل کردن توکن
                token.Revoke("Revoked by user");
                await _refreshTokenRepository.UpdateAsync(token);
                await _refreshTokenRepository.SaveChangesAsync();

                // ثبت لاگ
                _logger.LogInformation("Revoked refresh token for user {UserId}", token.UserId);
            }
            catch (Exception ex)
            {
                // ثبت خطا
                _logger.LogError(ex, "Failed to revoke refresh token");
                throw new RefreshTokenServiceException("Failed to revoke refresh token", ex);
            }
        }

        /// <inheritdoc/>
        /// <summary>
        /// باطل کردن تمامی توکن‌های یک کاربر
        /// </summary>
        public async Task RevokeAllTokensForUserAsync(Guid userId)
        {
            try
            {
                // یافتن تمامی توکن‌های فعال کاربر
                var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

                // باطل کردن هر توکن
                foreach (var token in tokens)
                {
                    token.Revoke("Revoked all tokens");
                    await _refreshTokenRepository.UpdateAsync(token);
                }

                // ذخیره تغییرات در دیتابیس
                await _refreshTokenRepository.SaveChangesAsync();

                // ثبت لاگ
                _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                // ثبت خطا
                _logger.LogError(ex, "Failed to revoke all refresh tokens for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to revoke all refresh tokens", ex);
            }
        }

        /// <summary>
        /// تولید یک توکن تصادفی امن برای استفاده به عنوان refresh token
        /// </summary>
        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32]; // 256 بیت امنیت
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes); // تولید بایت‌های تصادفی
            }

            // تبدیل به Base64 برای استفاده در API
            return Convert.ToBase64String(randomBytes);
        }
    }

    /// <summary>
    /// کلاس استثنا برای مدیریت خطاها در سرویس توکن رفرش
    /// </summary>
    public class RefreshTokenServiceException : Exception
    {
        public RefreshTokenServiceException(string message) : base(message) { }

        public RefreshTokenServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}