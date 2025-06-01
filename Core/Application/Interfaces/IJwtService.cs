using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس JWT
    /// این اینترفیس عملیات مربوط به مدیریت توکن‌های JWT و احراز هویت دو مرحله‌ای را تعریف می‌کند
    /// </summary>
    public interface IJwtService
    {
        string GenerateToken(User user);
        Task<string> GenerateTokenAsync(User user);
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User user);
        Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user);
        IDictionary<string, string> ValidateToken(string token);
        Guid GetUserIdFromToken(string token);
        string GetUserRoleFromToken(string token);
        (string secret, string qrCode) GenerateTwoFactorSecret(User user);
        bool ValidateTwoFactorCode(string secret, string code);
        bool ValidateTwoFactorToken(User user, string token);
        IEnumerable<string> GenerateRecoveryCodes();
        IEnumerable<Claim> GenerateClaims(User user);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken);
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);
        Task<IDictionary<string, string>> GetTokenClaimsAsync(string token);
        Task<DateTime> GetTokenExpirationAsync(string token);
        Task<bool> IsTokenRevokedAsync(string token);
        Task RevokeTokenAsync(string token);
        Task RevokeAllUserTokensAsync(Guid userId);
        string GenerateRefreshToken(User user);
        bool ValidateRefreshToken(User user, string refreshToken);
    }
}
