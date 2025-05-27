using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;

namespace Authorization_Login_Asp.Net.Infrastructure.Security
{
    /// <summary>
    /// کلاس تولید و اعتبارسنجی توکن JWT
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtTokenGenerator> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IMemoryCache _cache;

        public JwtTokenGenerator(
            IConfiguration configuration,
            ILogger<JwtTokenGenerator> logger,
            IRefreshTokenService refreshTokenService,
            IMemoryCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _refreshTokenService = refreshTokenService;
            _cache = cache;
            _jwtSettings = _configuration.GetSection("JwtSettings").Get<JwtSettings>();
        }

        /// <summary>
        /// تولید توکن JWT برای کاربر
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>توکن JWT و توکن بازیابی</returns>
        public async Task<(string Token, string RefreshToken)> GenerateTokensAsync(User user, string ipAddress)
        {
            try
            {
                var token = GenerateToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress);

                _logger.LogInformation("Generated new tokens for user {UserId}", user.Id);
                return (token, refreshToken.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {UserId}", user.Id);
                throw new JwtTokenException("Failed to generate tokens", ex);
            }
        }

        /// <summary>
        /// تولید توکن JWT
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <returns>توکن JWT</returns>
        public string GenerateToken(User user)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                    new Claim("email_verified", user.EmailVerified.ToString().ToLower()),
                    new Claim("two_factor_enabled", user.TwoFactorEnabled.ToString().ToLower()),
                    new Claim("token_type", "access_token")
                };

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                throw new JwtTokenException("Failed to generate token", ex);
            }
        }

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public bool ValidateToken(string token)
        {
            try
            {
                // بررسی وجود توکن در لیست توکن‌های لغو شده
                if (IsTokenRevoked(token))
                {
                    _logger.LogWarning("Attempted to use revoked token");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        /// <summary>
        /// استخراج اطلاعات کاربر از توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>اطلاعات کاربر</returns>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting principal from token");
                throw new JwtTokenException("Failed to get principal from token", ex);
            }
        }

        /// <summary>
        /// لغو توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        public void RevokeToken(string token)
        {
            try
            {
                var jti = GetTokenId(token);
                if (!string.IsNullOrEmpty(jti))
                {
                    var cacheKey = $"revoked_token_{jti}";
                    _cache.Set(cacheKey, true, TimeSpan.FromMinutes(_jwtSettings.ExpirationInMinutes));
                    _logger.LogInformation("Token revoked: {TokenId}", jti);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                throw new JwtTokenException("Failed to revoke token", ex);
            }
        }

        /// <summary>
        /// بررسی لغو شدن توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نتیجه بررسی</returns>
        public bool IsTokenRevoked(string token)
        {
            try
            {
                var jti = GetTokenId(token);
                if (string.IsNullOrEmpty(jti))
                    return false;

                var cacheKey = $"revoked_token_{jti}";
                return _cache.TryGetValue<bool>(cacheKey, out var isRevoked) && isRevoked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token revocation status");
                return true; // در صورت خطا، توکن را لغو شده در نظر می‌گیریم
            }
        }

        /// <summary>
        /// دریافت شناسه توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>شناسه توکن</returns>
        private string GetTokenId(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Id;
            }
            catch
            {
                return null;
            }
        }
    }

    public class JwtTokenException : Exception
    {
        public JwtTokenException(string message) : base(message) { }
        public JwtTokenException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 