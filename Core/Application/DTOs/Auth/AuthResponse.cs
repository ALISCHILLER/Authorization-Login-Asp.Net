using Authorization_Login_Asp.Net.Core.Application.DTOs.Users; // Assuming UserDto will be moved to Users folder
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public bool IsSuccess { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();
        public bool RequiresTwoFactor { get; set; }
        public List<string> Permissions { get; set; } = new();
        public string RefreshToken { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime? AccessTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string? UserIdFor2FA { get; set; }
        public string? Message { get; set; }
    }
}
