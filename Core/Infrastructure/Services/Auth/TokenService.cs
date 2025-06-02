using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Auth
{
    /// <summary>
    /// سرویس یکپارچه مدیریت توکن‌ها
    /// این سرویس شامل تمام عملیات مربوط به مدیریت توکن‌های JWT و Refresh است
    /// </summary>
    public class TokenService : BaseTokenService, ITokenService
    {
        public TokenService(
            ILogger<TokenService> logger,
            IMemoryCache cache,
            IOptions<JwtOptions> jwtOptions)
            : base(logger, cache, jwtOptions)
        {
        }

        /// <summary>
        /// ایجاد توکن‌های جدید برای کاربر
        /// </summary>
        public async Task<(string JwtToken, string RefreshToken)> GenerateTokensAsync(ClaimsPrincipal user)
        {
            return await ExecuteWithLoggingAsync("ایجاد توکن‌های جدید", async () =>
            {
                return await base.GenerateTokensAsync(user);
            });
        }

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            return await ExecuteWithLoggingAsync("اعتبارسنجی توکن", async () =>
            {
                return base.ValidateJwtToken(token);
            });
        }

        /// <summary>
        /// اعتبارسنجی توکن رفرش
        /// </summary>
        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            return await ExecuteWithLoggingAsync("اعتبارسنجی توکن رفرش", async () =>
            {
                return await base.ValidateRefreshTokenAsync(userId, refreshToken);
            });
        }

        /// <summary>
        /// حذف توکن رفرش
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string userId)
        {
            await ExecuteWithLoggingAsync("حذف توکن رفرش", async () =>
            {
                await base.RemoveRefreshTokenAsync(userId);
            });
        }

        /// <summary>
        /// به‌روزرسانی توکن‌ها
        /// </summary>
        public async Task<(string JwtToken, string RefreshToken)> RefreshTokensAsync(
            string userId,
            string refreshToken,
            ClaimsPrincipal user)
        {
            return await ExecuteWithLoggingAsync("به‌روزرسانی توکن‌ها", async () =>
            {
                // بررسی اعتبار توکن رفرش
                var isValid = await ValidateRefreshTokenAsync(userId, refreshToken);
                if (!isValid)
                {
                    throw new DomainException("توکن رفرش نامعتبر است");
                }

                // حذف توکن رفرش قبلی
                await RevokeRefreshTokenAsync(userId);

                // ایجاد توکن‌های جدید
                return await GenerateTokensAsync(user);
            });
        }

        /// <summary>
        /// خروج کاربر و حذف توکن‌ها
        /// </summary>
        public async Task LogoutAsync(string userId)
        {
            await ExecuteWithLoggingAsync("خروج کاربر", async () =>
            {
                await RevokeRefreshTokenAsync(userId);
            });
        }
    }
} 