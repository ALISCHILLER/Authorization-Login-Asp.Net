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
        Task<TokenResult> GenerateTokensAsync(User user, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// ایجاد توکن دسترسی
        /// </summary>
        Task<TokenResult> GenerateAccessTokenAsync(User user, IEnumerable<Claim> additionalClaims = null);

        /// <summary>
        /// ایجاد توکن بازنشانی
        /// </summary>
        Task<TokenResult> GenerateRefreshTokenAsync(User user, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// بررسی اعتبار توکن دسترسی
        /// </summary>
        Task<(bool IsValid, string Error)> ValidateAccessTokenAsync(string token);

        /// <summary>
        /// بررسی اعتبار توکن بازنشانی
        /// </summary>
        Task<(bool IsValid, string Error)> ValidateRefreshTokenAsync(string refreshToken, string ipAddress = null);

        /// <summary>
        /// باطل کردن توکن دسترسی
        /// </summary>
        Task<bool> RevokeAccessTokenAsync(string token, string reason = null);

        /// <summary>
        /// باطل کردن توکن بازنشانی
        /// </summary>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string reason = null);

        /// <summary>
        /// باطل کردن تمام توکن‌های کاربر
        /// </summary>
        Task<bool> RevokeAllUserTokensAsync(Guid userId, string reason = null);

        /// <summary>
        /// نوسازی توکن‌ها با استفاده از توکن بازنشانی
        /// </summary>
        Task<TokenResult> RefreshTokenAsync(string refreshToken, string ipAddress = null, string userAgent = null);
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
        Task<Guid?> GetUserIdFromTokenAsync(string token);

        /// <summary>
        /// دریافت نقش کاربر از توکن
        /// </summary>
        Task<string> GetUserRoleFromTokenAsync(string token);

        /// <summary>
        /// دریافت نام کاربری از توکن
        /// </summary>
        Task<string> GetUsernameFromTokenAsync(string token);

        /// <summary>
        /// دریافت آدرس IP از توکن
        /// </summary>
        Task<string> GetIpAddressFromTokenAsync(string token);

        /// <summary>
        /// دریافت مرورگر کاربر از توکن
        /// </summary>
        Task<string> GetUserAgentFromTokenAsync(string token);
        #endregion

        #region احراز هویت دو مرحله‌ای
        /// <summary>
        /// تولید کلید محرمانه و کد QR برای احراز هویت دو مرحله‌ای
        /// </summary>
        Task<(string Secret, string QrCodeUri)> GenerateTwoFactorSecretAsync(User user);

        /// <summary>
        /// بررسی اعتبار کد احراز هویت دو مرحله‌ای
        /// </summary>
        Task<bool> ValidateTwoFactorCodeAsync(string secret, string code);

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        Task<IEnumerable<string>> GenerateRecoveryCodesAsync(int count = 8);

        /// <summary>
        /// بررسی اعتبار کد بازیابی
        /// </summary>
        Task<bool> ValidateRecoveryCodeAsync(User user, string code);

        /// <summary>
        /// غیرفعال کردن احراز هویت دو مرحله‌ای
        /// </summary>
        Task<bool> DisableTwoFactorAsync(User user);
        #endregion

        #region مدیریت نشست‌ها
        /// <summary>
        /// دریافت لیست نشست‌های فعال کاربر
        /// </summary>
        Task<IEnumerable<UserSession>> GetActiveSessionsAsync(Guid userId);

        /// <summary>
        /// پایان دادن به نشست
        /// </summary>
        Task<bool> TerminateSessionAsync(Guid sessionId, string reason = null);

        /// <summary>
        /// پایان دادن به تمام نشست‌های کاربر به جز نشست فعلی
        /// </summary>
        Task<bool> TerminateOtherSessionsAsync(Guid userId, Guid currentSessionId, string reason = null);
        #endregion
    }

    /// <summary>
    /// نتیجه ایجاد توکن
    /// </summary>
    public class TokenResult
    {
        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن بازنشانی
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// زمان انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// زمان انقضای توکن بازنشانی
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// نوع توکن
        /// </summary>
        public string TokenType { get; set; } = "Bearer";
    }

    /// <summary>
    /// اطلاعات نشست کاربر
    /// </summary>
    public class UserSession
    {
        /// <summary>
        /// شناسه نشست
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// مرورگر کاربر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// زمان شروع نشست
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// آخرین زمان فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// زمان پایان نشست
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// دلیل پایان نشست
        /// </summary>
        public string EndReason { get; set; }
    }
} 