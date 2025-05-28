using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly int _tokenExpirationDays;
        private readonly int _maxActiveTokensPerUser;

        public RefreshTokenService(
            IConfiguration configuration,
            ILogger<RefreshTokenService> logger,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenExpirationDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]);
            _maxActiveTokensPerUser = int.Parse(_configuration["JwtSettings:MaxActiveRefreshTokensPerUser"]);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId, string ipAddress)
        {
            try
            {
                // Revoke all existing refresh tokens for this user if max limit reached
                var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
                if (activeTokens.Count >= _maxActiveTokensPerUser)
                {
                    foreach (var token in activeTokens)
                    {
                        await RevokeRefreshTokenAsync(token.Token);
                    }
                }

                var token = new RefreshToken
                {
                    Token = GenerateRandomToken(),
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(_tokenExpirationDays),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress,
                    IsRevoked = false,
                    ReplacedByToken = null,
                    ReasonRevoked = null
                };

                await _refreshTokenRepository.AddAsync(token);
                _logger.LogInformation("Generated new refresh token for user {UserId}", userId);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate refresh token for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to generate refresh token", ex);
            }
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                return refreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get refresh token");
                throw new RefreshTokenServiceException("Failed to get refresh token", ex);
            }
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress = null, string reason = null)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReasonRevoked = reason;

                await _refreshTokenRepository.UpdateAsync(refreshToken);
                _logger.LogInformation("Revoked refresh token for user {UserId}", refreshToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke refresh token");
                throw new RefreshTokenServiceException("Failed to revoke refresh token", ex);
            }
        }

        public async Task RevokeAllRefreshTokensForUserAsync(int userId, string ipAddress = null, string reason = null)
        {
            try
            {
                var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
                foreach (var token in tokens)
                {
                    await RevokeRefreshTokenAsync(token.Token, ipAddress, reason);
                }
                _logger.LogInformation("Revoked all refresh tokens for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke all refresh tokens for user {UserId}", userId);
                throw new RefreshTokenServiceException("Failed to revoke all refresh tokens", ex);
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    return false;
                }

                if (refreshToken.IsRevoked)
                {
                    _logger.LogWarning("Attempted to use revoked refresh token");
                    return false;
                }

                if (refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Attempted to use expired refresh token");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate refresh token");
                return false;
            }
        }

        public async Task RotateRefreshTokenAsync(string token, string ipAddress)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
                if (refreshToken == null)
                {
                    throw new RefreshTokenServiceException("Invalid refresh token");
                }

                // Generate new token
                var newToken = await GenerateRefreshTokenAsync(refreshToken.UserId, ipAddress);

                // Revoke old token
                await RevokeRefreshTokenAsync(token, ipAddress, "Rotated");

                // Update old token with new token reference
                refreshToken.ReplacedByToken = newToken.Token;
                await _refreshTokenRepository.UpdateAsync(refreshToken);

                _logger.LogInformation("Rotated refresh token for user {UserId}", refreshToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate refresh token");
                throw new RefreshTokenServiceException("Failed to rotate refresh token", ex);
            }
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public Task<string> GenerateRefreshTokenAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllTokensForUserAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }

    public class RefreshTokenServiceException : Exception
    {
        public RefreshTokenServiceException(string message) : base(message) { }
        public RefreshTokenServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}