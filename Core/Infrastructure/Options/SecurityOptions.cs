namespace Authorization_Login_Asp.Net.Core.Infrastructure.Options
{
    public class SecurityOptions
    {
        public int MinimumPasswordLength { get; set; } = 8;
        public int PasswordHashWorkFactor { get; set; } = 12;
        public int TwoFactorCodeExpiryMinutes { get; set; } = 5;
        public int PasswordResetCodeExpiryMinutes { get; set; } = 30;
        public int LockoutDurationMinutes { get; set; } = 30;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public bool RequireUniqueEmail { get; set; } = true;
        public bool RequireConfirmedEmail { get; set; } = true;
        public bool RequireConfirmedPhoneNumber { get; set; } = false;
    }
} 