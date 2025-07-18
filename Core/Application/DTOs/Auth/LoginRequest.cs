using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceToken { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
    }
}
