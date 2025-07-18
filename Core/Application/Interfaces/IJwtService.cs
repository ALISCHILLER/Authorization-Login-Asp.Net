using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<string> GenerateAccessTokenAsync(User user);
        Task<RefreshToken> GenerateRefreshTokenAsync(User user, string? ipAddress = null);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
        bool ValidateToken(string token);
    }
}
