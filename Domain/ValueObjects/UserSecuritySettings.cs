using System;

namespace Authorization_Login_Asp.Net.Domain.ValueObjects
{
    public class UserSecuritySettings
    {
        public bool RequirePasswordChange { get; set; }
        public DateTime? PasswordExpiryDate { get; set; }
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int AccountLockoutDurationMinutes { get; set; } = 30;
        public bool RequireStrongPassword { get; set; } = true;
        public int PasswordHistoryCount { get; set; } = 5;
        public bool RequireUniquePasswords { get; set; } = true;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public bool RequireIpValidation { get; set; }
        public bool RequireDeviceValidation { get; set; }
        public bool RequireLocationValidation { get; set; }
        public bool RequireEmailVerification { get; set; } = true;
        public bool RequirePhoneVerification { get; set; }
        public bool RequireTwoFactorForNewDevices { get; set; }
        public bool RequireTwoFactorForSensitiveOperations { get; set; }
        public bool RequireTwoFactorForAdminAccess { get; set; } = true;
        public bool RequireTwoFactorForExternalAccess { get; set; }
        public bool RequireTwoFactorForHighValueTransactions { get; set; }
        public bool RequireTwoFactorForPasswordChange { get; set; } = true;
        public bool RequireTwoFactorForEmailChange { get; set; } = true;
        public bool RequireTwoFactorForPhoneChange { get; set; } = true;
        public bool RequireTwoFactorForSecuritySettingsChange { get; set; } = true;
    }
} 