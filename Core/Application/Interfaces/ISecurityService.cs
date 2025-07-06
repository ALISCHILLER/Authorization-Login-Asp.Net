using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ISecurityService
    {
        Task<string> HashPasswordAsync(string password);
        Task<bool> VerifyPasswordAsync(string password, string hash);
        Task<string> GenerateJwtTokenAsync(Guid userId, string userName, string[] roles);
        Task<bool> ValidateTokenAsync(string token);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task<bool> IsPasswordExpiredAsync(Guid userId);
        Task<bool> IsAccountLockedAsync(Guid userId);
        Task<int> GetFailedLoginAttemptsAsync(Guid userId);
        Task ResetFailedLoginAttemptsAsync(Guid userId);
        Task IncrementFailedLoginAttemptsAsync(Guid userId);
        Task<bool> IsTwoFactorEnabledAsync(Guid userId);
        Task<string> GenerateTwoFactorTokenAsync(Guid userId);
        Task<bool> ValidateTwoFactorTokenAsync(Guid userId, string token);
        Task EnableAsync(User user, TwoFactorType type);
        Task DisableAsync(User user);
        Task SendCodeAsync(User user);
        string GenerateTemporaryPassword();
    }
}
