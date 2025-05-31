using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی تولید و اعتبارسنجی توکن JWT
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<JwtTokenGenerator> _logger;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public JwtTokenGenerator(
            IOptions<JwtOptions> jwtOptions,
            ILogger<JwtTokenGenerator> logger,
            IRefreshTokenService refreshTokenService,
            IMemoryCache cache)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// تولید توکن JWT و توکن بازیابی برای کاربر
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>توکن JWT و توکن بازیابی</returns>
        public async Task<(string Token, string RefreshToken)> GenerateTokensAsync(User user, string ipAddress)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentNullException(nameof(ipAddress));

            try
            {
                var token = GenerateToken(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id);

                _logger.LogInformation("Generated new tokens for user {UserId}", user.Id);
                return (token, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating tokens for user {UserId}", user.Id);
                throw new JwtTokenException("Failed to generate tokens", ex);
            }
        }

        /// <inheritdoc/>
        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var claims = GenerateClaims(user);
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

                var token = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    expires: expires,
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

        /// <inheritdoc/>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            try
            {
                // بررسی وجود توکن در لیست توکن‌های لغو شده
                if (IsTokenRevoked(token))
                {
                    _logger.LogWarning("Attempted to use revoked token");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }

        /// <inheritdoc/>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("توکن نامعتبر است");
                }

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
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            try
            {
                var jti = GetTokenId(token);
                if (!string.IsNullOrEmpty(jti))
                {
                    var cacheKey = $"revoked_token_{jti}";
                    _cache.Set(cacheKey, true, TimeSpan.FromMinutes(_jwtOptions.ExpiryMinutes));
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
            if (string.IsNullOrWhiteSpace(token))
                return true;

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
        /// تولید کلیم‌های کاربر
        /// </summary>
        private IEnumerable<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Email, user.Email.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim("email_verified", user.IsEmailVerified.ToString().ToLower()),
                new Claim("two_factor_enabled", user.TwoFactorEnabled.ToString().ToLower()),
                new Claim("token_type", "access_token")
            };

            // اضافه کردن نقش کاربر
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));

                // اضافه کردن دسترسی‌های کاربر از طریق نقش
                if (user.Role.Permissions != null)
                {
                    foreach (var permission in user.Role.Permissions)
                    {
                        if (permission.IsActive)
                        {
                            claims.Add(new Claim("Permission", permission.Name));
                        }
                    }
                }
            }

            return claims;
        }

        /// <summary>
        /// دریافت شناسه توکن JWT
        /// </summary>
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

    /// <summary>
    /// استثنای مربوط به توکن JWT
    /// </summary>
    public class JwtTokenException : Exception
    {
        public JwtTokenException(string message) : base(message) { }
        public JwtTokenException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 