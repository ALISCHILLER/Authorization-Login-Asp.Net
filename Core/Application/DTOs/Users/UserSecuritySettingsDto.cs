namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class UserSecuritySettingsDto
    {
        public bool TwoFactorEnabled { get; set; }
        public int MaxFailedLoginAttempts { get; set; }
        public int LockoutDurationMinutes { get; set; }
        public bool HasTwoFactorEnabled { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public bool RequiresPasswordChange { get; set; }
    }
}
