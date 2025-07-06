using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces.Services
{
    public interface IUserAuthenticationService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> ValidateTwoFactorAsync(TwoFactorRequest request, CancellationToken cancellationToken = default);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
        Task<bool> RevokeTokenAsync(string token, string ipAddress);
        Task<bool> ValidateTokenAsync(string token);
        Task<User> GetUserFromTokenAsync(string token);
        Task<bool> IsEmailConfirmedAsync(string email);
        Task<bool> IsPhoneNumberConfirmedAsync(string phoneNumber);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> IsLockedOutAsync(User user);
    }
}