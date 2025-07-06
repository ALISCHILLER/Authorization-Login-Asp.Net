using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync();
        Task<ClaimsPrincipal> GetPrincipalFromExpiredTokenAsync(string token);
        Task<bool> ValidateTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<IEnumerable<Claim>> GetClaimsAsync(User user);
        Task<DateTime> GetTokenExpirationAsync(string token);
    }
} 