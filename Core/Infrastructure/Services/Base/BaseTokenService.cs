using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base
{
    /// <summary>
    /// کلاس پایه برای سرویس‌های مدیریت توکن
    /// این کلاس شامل متدهای مشترک برای مدیریت توکن‌های JWT و Refresh است
    /// </summary>
    public abstract class BaseTokenService : BaseService
    {
        protected readonly JwtOptions _jwtOptions;

        protected BaseTokenService(
            ILogger logger,
            IMemoryCache cache,
            IOptions<JwtOptions> jwtOptions,
            int cacheExpirationMinutes = 30)
            : base(logger, cache, cacheExpirationMinutes)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        /// <summary>
        /// ایجاد توکن JWT با استفاده از اطلاعات کاربر
        /// </summary>
        protected string GenerateJwtToken(ClaimsPrincipal user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                new Claim(ClaimTypes.Name, user.FindFirst(ClaimTypes.Name)?.Value),
                new Claim(ClaimTypes.Email, user.FindFirst(ClaimTypes.Email)?.Value),
                new Claim(ClaimTypes.Role, user.FindFirst(ClaimTypes.Role)?.Value)
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        protected bool ValidateJwtToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

            try
            {
                tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// ایجاد توکن رفرش
        /// </summary>
        protected string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// ذخیره توکن رفرش در کش
        /// </summary>
        protected async Task StoreRefreshTokenAsync(string userId, string refreshToken)
        {
            var cacheKey = $"RefreshToken_{userId}";
            await Task.Run(() => _cache.Set(cacheKey, refreshToken, _cacheOptions));
        }

        /// <summary>
        /// بررسی اعتبار توکن رفرش
        /// </summary>
        protected async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var cacheKey = $"RefreshToken_{userId}";
            if (_cache.TryGetValue(cacheKey, out string storedToken))
            {
                return storedToken == refreshToken;
            }
            return false;
        }

        /// <summary>
        /// حذف توکن رفرش از کش
        /// </summary>
        protected async Task RemoveRefreshTokenAsync(string userId)
        {
            var cacheKey = $"RefreshToken_{userId}";
            await Task.Run(() => _cache.Remove(cacheKey));
        }

        /// <summary>
        /// ایجاد توکن‌های جدید (JWT و Refresh)
        /// </summary>
        protected async Task<(string JwtToken, string RefreshToken)> GenerateTokensAsync(ClaimsPrincipal user)
        {
            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await StoreRefreshTokenAsync(userId, refreshToken);
            }

            return (jwtToken, refreshToken);
        }
    }
} 