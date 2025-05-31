using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس توکن رفرش
    /// </summary>
    public class RefreshTokenService : IRefreshTokenService
    {
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
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            
            _tokenExpirationDays = _configuration.GetValue<int>("AppSettings:JwtSettings:RefreshTokenExpiryDays", 7);
            _maxActiveTokensPerUser = _configuration.GetValue<int>("AppSettings:JwtSettings:MaxActiveRefreshTokensPerUser", 5);
        }

        /// <inheritdoc/>
        public async Task<string> GenerateRefreshTokenAsync(Guid userId)
        {
            try
            {
                // بررسی تعداد توکن‌های فعال کاربر
                var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
                if (activeTokens.Count >= _maxActiveTokensPerUser)
                {
                    // باطل کردن تمام توکن‌های قبلی
                    foreach (var token in activeTokens)
                    {
                        await RevokeRefreshTokenAsync(token.Token);
                    }
                }

                var refreshToken = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Token = GenerateRandomToken(),
                    ExpiresAt = DateTime.UtcNow.AddDays(_tokenExpirationDays),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                };

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                _logger.LogInformation("Generated new refresh token for user {UserId}", userId);
                return refreshToken.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate refresh token for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to generate refresh token", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (token == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

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

                _logger.LogInformation("Refreshed token for user {UserId}", token.UserId);
                return newToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh token");
                throw new RefreshTokenServiceException("Failed to refresh token", ex);
            }
        }

        /// <inheritdoc/>
        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (token == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                token.Revoke("Revoked by user");
                await _refreshTokenRepository.UpdateAsync(token);
                await _refreshTokenRepository.SaveChangesAsync();

                _logger.LogInformation("Revoked refresh token for user {UserId}", token.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke refresh token");
                throw new RefreshTokenServiceException("Failed to revoke refresh token", ex);
            }
        }

        /// <inheritdoc/>
        public async Task RevokeAllTokensForUserAsync(Guid userId)
        {
            try
            {
                var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
                foreach (var token in tokens)
                {
                    token.Revoke("Revoked all tokens");
                    await _refreshTokenRepository.UpdateAsync(token);
                }
                await _refreshTokenRepository.SaveChangesAsync();

                _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke all refresh tokens for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to revoke all refresh tokens", ex);
            }
        }

        /// <summary>
        /// تولید توکن تصادفی
        /// </summary>
        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }
    }

    /// <summary>
    /// استثنای مربوط به سرویس توکن رفرش
    /// </summary>
    public class RefreshTokenServiceException : Exception
    {
        public RefreshTokenServiceException(string message) : base(message) { }
        public RefreshTokenServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}