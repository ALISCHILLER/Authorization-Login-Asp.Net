using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ITwoFactorAuthenticator
    {
        string GenerateSecretKey();
        string GenerateCode(string secretKey);
        bool ValidateCode(string secretKey, string code);
        string[] GenerateRecoveryCodes(int count = 10);
        Task SendCodeAsync(User user, string code);
        string GenerateQrCode(string secretKey, string email, string? issuer = null);
        bool ValidateRecoveryCode(string recoveryCode, IEnumerable<string> storedCodes);
    }
}
