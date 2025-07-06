using System;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Models
{
    /// <summary>
    /// مدل پایه برای اطلاعات کاربر در احراز هویت
    /// </summary>
    public abstract class AuthUserBaseDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserName => Username;
    }

    /// <summary>
    /// مدل DTO دستگاه کاربر
    /// </summary>
    public class UserDeviceDto : AuthUserBaseDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; }
    }

    /// <summary>
    /// مدل DTO توکن رفرش
    /// </summary>
    public class RefreshTokenDto : AuthUserBaseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }

    /// <summary>
    /// مدل DTO کدهای بازیابی دو مرحله‌ای
    /// </summary>
    public class TwoFactorRecoveryCodeDto : AuthUserBaseDto
    {
        public string Code { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}