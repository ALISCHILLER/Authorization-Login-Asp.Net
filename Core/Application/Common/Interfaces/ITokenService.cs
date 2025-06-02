using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Interfaces
{
    /// <summary>
    /// رابط جامع مدیریت توکن‌ها
    /// این رابط تمام عملیات مربوط به مدیریت توکن‌های JWT، توکن‌های بازنشانی و احراز هویت دو مرحله‌ای را تعریف می‌کند
    /// </summary>
    public interface ITokenService
    {
        #region توکن‌های دسترسی و بازنشانی
        /// <summary>
        /// ایجاد توکن دسترسی و بازنشانی برای کاربر
        /// </summary>
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user, string ipAddress = null);

        /// <summary>
        /// ایجاد توکن دسترسی
        /// </summary>
        Task<string> GenerateAccessTokenAsync(User user);

        /// <summary>
        /// ایجاد توکن بازنشانی
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(User user);

        /// <summary>
        /// بررسی اعتبار توکن دسترسی
        /// </summary>
        Task<bool> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// بررسی اعتبار توکن بازنشانی
        /// </summary>
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// باطل کردن توکن دسترسی
        /// </summary>
        Task RevokeAccessTokenAsync(string token);

        /// <summary>
        /// باطل کردن توکن بازنشانی
        /// </summary>
        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// باطل کردن تمام توکن‌های کاربر
        /// </summary>
        Task RevokeAllUserTokensAsync(Guid userId);
        #endregion

        #region اطلاعات توکن
        /// <summary>
        /// استخراج اطلاعات کاربر از توکن
        /// </summary>
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);

        /// <summary>
        /// دریافت ادعاهای توکن
        /// </summary>
        Task<IDictionary<string, string>> GetTokenClaimsAsync(string token);

        /// <summary>
        /// دریافت زمان انقضای توکن
        /// </summary>
        Task<DateTime> GetTokenExpirationAsync(string token);

        /// <summary>
        /// دریافت شناسه کاربر از توکن
        /// </summary>
        Guid GetUserIdFromToken(string token);

        /// <summary>
        /// دریافت نقش کاربر از توکن
        /// </summary>
        string GetUserRoleFromToken(string token);
        #endregion

        #region احراز هویت دو مرحله‌ای
        /// <summary>
        /// تولید کلید محرمانه و کد QR برای احراز هویت دو مرحله‌ای
        /// </summary>
        (string Secret, string QrCode) GenerateTwoFactorSecret(User user);

        /// <summary>
        /// بررسی اعتبار کد احراز هویت دو مرحله‌ای
        /// </summary>
        bool ValidateTwoFactorCode(string secret, string code);

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        IEnumerable<string> GenerateRecoveryCodes();
        #endregion
    }

    /// <summary>
    /// نتیجه ایجاد توکن
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// توکن ایجاد شده
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// زمان انقضای توکن
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// نوع توکن
        /// </summary>
        public TokenType Type { get; set; }
    }

    /// <summary>
    /// نوع توکن
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// توکن دسترسی
        /// </summary>
        AccessToken,

        /// <summary>
        /// توکن بازنشانی
        /// </summary>
        RefreshToken
    }
} 