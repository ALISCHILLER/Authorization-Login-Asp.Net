using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// Value Object برای تنظیمات امنیتی
    /// این کلاس شامل تنظیمات امنیتی کاربر مانند تعداد تلاش‌های مجاز، مدت زمان قفل و غیره است
    /// </summary>
    public class SecuritySettings : ValueObject
    {
        // تنظیمات پایه
        public int MaxFailedLoginAttempts { get; private set; } = 5;
        public int LockoutDurationMinutes { get; private set; } = 30;
        public int PasswordExpiryDays { get; private set; } = 90;
        public int SessionTimeoutMinutes { get; private set; } = 60;
        public bool RequireStrongPassword { get; private set; } = true;
        public int PasswordHistoryCount { get; private set; } = 5;
        public bool RequireUniquePasswords { get; private set; } = true;

        // تنظیمات تأیید
        public bool RequireEmailVerification { get; private set; } = true;
        public bool RequirePhoneVerification { get; private set; }
        public bool RequireIpValidation { get; private set; }
        public bool RequireDeviceValidation { get; private set; }
        public bool RequireLocationValidation { get; private set; }

        // تنظیمات احراز هویت دو مرحله‌ای
        public bool RequireTwoFactor { get; private set; }
        public bool RequireTwoFactorForNewDevices { get; private set; }
        public bool RequireTwoFactorForSensitiveOperations { get; private set; }
        public bool RequireTwoFactorForAdminAccess { get; private set; } = true;
        public bool RequireTwoFactorForExternalAccess { get; private set; }
        public bool RequireTwoFactorForHighValueTransactions { get; private set; }
        public bool RequireTwoFactorForPasswordChange { get; private set; } = true;
        public bool RequireTwoFactorForEmailChange { get; private set; } = true;
        public bool RequireTwoFactorForPhoneChange { get; private set; } = true;
        public bool RequireTwoFactorForSecuritySettingsChange { get; private set; } = true;

        // تنظیمات دستگاه
        public int MaxActiveDevices { get; private set; } = 5;
        public bool NotifyOnNewDevice { get; private set; } = true;

        // تنظیمات اعلان
        public bool NotifyOnPasswordChange { get; private set; } = true;
        public bool NotifyOnEmailChange { get; private set; } = true;
        public bool NotifyOnPhoneChange { get; private set; } = true;

        // تنظیمات اضافی
        public bool RequirePasswordChange { get; private set; }
        public DateTime? PasswordExpiryDate { get; private set; }

        protected SecuritySettings() { }

        public static SecuritySettings Create(
            int? maxFailedLoginAttempts = null,
            int? lockoutDurationMinutes = null,
            int? passwordExpiryDays = null,
            int? sessionTimeoutMinutes = null,
            bool? requireStrongPassword = null,
            int? passwordHistoryCount = null,
            bool? requireUniquePasswords = null,
            bool? requireEmailVerification = null,
            bool? requirePhoneVerification = null,
            bool? requireIpValidation = null,
            bool? requireDeviceValidation = null,
            bool? requireLocationValidation = null,
            bool? requireTwoFactor = null,
            bool? requireTwoFactorForNewDevices = null,
            bool? requireTwoFactorForSensitiveOperations = null,
            bool? requireTwoFactorForAdminAccess = null,
            bool? requireTwoFactorForExternalAccess = null,
            bool? requireTwoFactorForHighValueTransactions = null,
            bool? requireTwoFactorForPasswordChange = null,
            bool? requireTwoFactorForEmailChange = null,
            bool? requireTwoFactorForPhoneChange = null,
            bool? requireTwoFactorForSecuritySettingsChange = null,
            int? maxActiveDevices = null,
            bool? notifyOnNewDevice = null,
            bool? notifyOnPasswordChange = null,
            bool? notifyOnEmailChange = null,
            bool? notifyOnPhoneChange = null,
            bool? requirePasswordChange = null,
            DateTime? passwordExpiryDate = null)
        {
            var settings = new SecuritySettings();

            if (maxFailedLoginAttempts.HasValue)
                settings.MaxFailedLoginAttempts = ValidatePositiveNumber(maxFailedLoginAttempts.Value, nameof(maxFailedLoginAttempts));

            if (lockoutDurationMinutes.HasValue)
                settings.LockoutDurationMinutes = ValidatePositiveNumber(lockoutDurationMinutes.Value, nameof(lockoutDurationMinutes));

            if (passwordExpiryDays.HasValue)
                settings.PasswordExpiryDays = ValidatePositiveNumber(passwordExpiryDays.Value, nameof(passwordExpiryDays));

            if (sessionTimeoutMinutes.HasValue)
                settings.SessionTimeoutMinutes = ValidatePositiveNumber(sessionTimeoutMinutes.Value, nameof(sessionTimeoutMinutes));

            if (passwordHistoryCount.HasValue)
                settings.PasswordHistoryCount = ValidatePositiveNumber(passwordHistoryCount.Value, nameof(passwordHistoryCount));

            if (maxActiveDevices.HasValue)
                settings.MaxActiveDevices = ValidatePositiveNumber(maxActiveDevices.Value, nameof(maxActiveDevices));

            // تنظیمات بولی
            if (requireStrongPassword.HasValue)
                settings.RequireStrongPassword = requireStrongPassword.Value;

            if (requireUniquePasswords.HasValue)
                settings.RequireUniquePasswords = requireUniquePasswords.Value;

            if (requireEmailVerification.HasValue)
                settings.RequireEmailVerification = requireEmailVerification.Value;

            if (requirePhoneVerification.HasValue)
                settings.RequirePhoneVerification = requirePhoneVerification.Value;

            if (requireIpValidation.HasValue)
                settings.RequireIpValidation = requireIpValidation.Value;

            if (requireDeviceValidation.HasValue)
                settings.RequireDeviceValidation = requireDeviceValidation.Value;

            if (requireLocationValidation.HasValue)
                settings.RequireLocationValidation = requireLocationValidation.Value;

            if (requireTwoFactor.HasValue)
                settings.RequireTwoFactor = requireTwoFactor.Value;

            if (requireTwoFactorForNewDevices.HasValue)
                settings.RequireTwoFactorForNewDevices = requireTwoFactorForNewDevices.Value;

            if (requireTwoFactorForSensitiveOperations.HasValue)
                settings.RequireTwoFactorForSensitiveOperations = requireTwoFactorForSensitiveOperations.Value;

            if (requireTwoFactorForAdminAccess.HasValue)
                settings.RequireTwoFactorForAdminAccess = requireTwoFactorForAdminAccess.Value;

            if (requireTwoFactorForExternalAccess.HasValue)
                settings.RequireTwoFactorForExternalAccess = requireTwoFactorForExternalAccess.Value;

            if (requireTwoFactorForHighValueTransactions.HasValue)
                settings.RequireTwoFactorForHighValueTransactions = requireTwoFactorForHighValueTransactions.Value;

            if (requireTwoFactorForPasswordChange.HasValue)
                settings.RequireTwoFactorForPasswordChange = requireTwoFactorForPasswordChange.Value;

            if (requireTwoFactorForEmailChange.HasValue)
                settings.RequireTwoFactorForEmailChange = requireTwoFactorForEmailChange.Value;

            if (requireTwoFactorForPhoneChange.HasValue)
                settings.RequireTwoFactorForPhoneChange = requireTwoFactorForPhoneChange.Value;

            if (requireTwoFactorForSecuritySettingsChange.HasValue)
                settings.RequireTwoFactorForSecuritySettingsChange = requireTwoFactorForSecuritySettingsChange.Value;

            if (notifyOnNewDevice.HasValue)
                settings.NotifyOnNewDevice = notifyOnNewDevice.Value;

            if (notifyOnPasswordChange.HasValue)
                settings.NotifyOnPasswordChange = notifyOnPasswordChange.Value;

            if (notifyOnEmailChange.HasValue)
                settings.NotifyOnEmailChange = notifyOnEmailChange.Value;

            if (notifyOnPhoneChange.HasValue)
                settings.NotifyOnPhoneChange = notifyOnPhoneChange.Value;

            if (requirePasswordChange.HasValue)
                settings.RequirePasswordChange = requirePasswordChange.Value;

            if (passwordExpiryDate.HasValue)
                settings.PasswordExpiryDate = passwordExpiryDate.Value;

            return settings;
        }

        private static int ValidatePositiveNumber(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentException("مقدار باید بزرگتر از صفر باشد", paramName);
            return value;
        }

        public void Update(
            int? maxFailedLoginAttempts = null,
            int? lockoutDurationMinutes = null,
            int? passwordExpiryDays = null,
            int? sessionTimeoutMinutes = null,
            bool? requireStrongPassword = null,
            int? passwordHistoryCount = null,
            bool? requireUniquePasswords = null,
            bool? requireEmailVerification = null,
            bool? requirePhoneVerification = null,
            bool? requireIpValidation = null,
            bool? requireDeviceValidation = null,
            bool? requireLocationValidation = null,
            bool? requireTwoFactor = null,
            bool? requireTwoFactorForNewDevices = null,
            bool? requireTwoFactorForSensitiveOperations = null,
            bool? requireTwoFactorForAdminAccess = null,
            bool? requireTwoFactorForExternalAccess = null,
            bool? requireTwoFactorForHighValueTransactions = null,
            bool? requireTwoFactorForPasswordChange = null,
            bool? requireTwoFactorForEmailChange = null,
            bool? requireTwoFactorForPhoneChange = null,
            bool? requireTwoFactorForSecuritySettingsChange = null,
            int? maxActiveDevices = null,
            bool? notifyOnNewDevice = null,
            bool? notifyOnPasswordChange = null,
            bool? notifyOnEmailChange = null,
            bool? notifyOnPhoneChange = null,
            bool? requirePasswordChange = null,
            DateTime? passwordExpiryDate = null)
        {
            if (maxFailedLoginAttempts.HasValue)
                MaxFailedLoginAttempts = ValidatePositiveNumber(maxFailedLoginAttempts.Value, nameof(maxFailedLoginAttempts));

            if (lockoutDurationMinutes.HasValue)
                LockoutDurationMinutes = ValidatePositiveNumber(lockoutDurationMinutes.Value, nameof(lockoutDurationMinutes));

            if (passwordExpiryDays.HasValue)
                PasswordExpiryDays = ValidatePositiveNumber(passwordExpiryDays.Value, nameof(passwordExpiryDays));

            if (sessionTimeoutMinutes.HasValue)
                SessionTimeoutMinutes = ValidatePositiveNumber(sessionTimeoutMinutes.Value, nameof(sessionTimeoutMinutes));

            if (passwordHistoryCount.HasValue)
                PasswordHistoryCount = ValidatePositiveNumber(passwordHistoryCount.Value, nameof(passwordHistoryCount));

            if (maxActiveDevices.HasValue)
                MaxActiveDevices = ValidatePositiveNumber(maxActiveDevices.Value, nameof(maxActiveDevices));

            // تنظیمات بولی
            if (requireStrongPassword.HasValue)
                RequireStrongPassword = requireStrongPassword.Value;

            if (requireUniquePasswords.HasValue)
                RequireUniquePasswords = requireUniquePasswords.Value;

            if (requireEmailVerification.HasValue)
                RequireEmailVerification = requireEmailVerification.Value;

            if (requirePhoneVerification.HasValue)
                RequirePhoneVerification = requirePhoneVerification.Value;

            if (requireIpValidation.HasValue)
                RequireIpValidation = requireIpValidation.Value;

            if (requireDeviceValidation.HasValue)
                RequireDeviceValidation = requireDeviceValidation.Value;

            if (requireLocationValidation.HasValue)
                RequireLocationValidation = requireLocationValidation.Value;

            if (requireTwoFactor.HasValue)
                RequireTwoFactor = requireTwoFactor.Value;

            if (requireTwoFactorForNewDevices.HasValue)
                RequireTwoFactorForNewDevices = requireTwoFactorForNewDevices.Value;

            if (requireTwoFactorForSensitiveOperations.HasValue)
                RequireTwoFactorForSensitiveOperations = requireTwoFactorForSensitiveOperations.Value;

            if (requireTwoFactorForAdminAccess.HasValue)
                RequireTwoFactorForAdminAccess = requireTwoFactorForAdminAccess.Value;

            if (requireTwoFactorForExternalAccess.HasValue)
                RequireTwoFactorForExternalAccess = requireTwoFactorForExternalAccess.Value;

            if (requireTwoFactorForHighValueTransactions.HasValue)
                RequireTwoFactorForHighValueTransactions = requireTwoFactorForHighValueTransactions.Value;

            if (requireTwoFactorForPasswordChange.HasValue)
                RequireTwoFactorForPasswordChange = requireTwoFactorForPasswordChange.Value;

            if (requireTwoFactorForEmailChange.HasValue)
                RequireTwoFactorForEmailChange = requireTwoFactorForEmailChange.Value;

            if (requireTwoFactorForPhoneChange.HasValue)
                RequireTwoFactorForPhoneChange = requireTwoFactorForPhoneChange.Value;

            if (requireTwoFactorForSecuritySettingsChange.HasValue)
                RequireTwoFactorForSecuritySettingsChange = requireTwoFactorForSecuritySettingsChange.Value;

            if (notifyOnNewDevice.HasValue)
                NotifyOnNewDevice = notifyOnNewDevice.Value;

            if (notifyOnPasswordChange.HasValue)
                NotifyOnPasswordChange = notifyOnPasswordChange.Value;

            if (notifyOnEmailChange.HasValue)
                NotifyOnEmailChange = notifyOnEmailChange.Value;

            if (notifyOnPhoneChange.HasValue)
                NotifyOnPhoneChange = notifyOnPhoneChange.Value;

            if (requirePasswordChange.HasValue)
                RequirePasswordChange = requirePasswordChange.Value;

            if (passwordExpiryDate.HasValue)
                PasswordExpiryDate = passwordExpiryDate.Value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return MaxFailedLoginAttempts;
            yield return LockoutDurationMinutes;
            yield return PasswordExpiryDays;
            yield return SessionTimeoutMinutes;
            yield return RequireStrongPassword;
            yield return PasswordHistoryCount;
            yield return RequireUniquePasswords;
            yield return RequireEmailVerification;
            yield return RequirePhoneVerification;
            yield return RequireIpValidation;
            yield return RequireDeviceValidation;
            yield return RequireLocationValidation;
            yield return RequireTwoFactor;
            yield return RequireTwoFactorForNewDevices;
            yield return RequireTwoFactorForSensitiveOperations;
            yield return RequireTwoFactorForAdminAccess;
            yield return RequireTwoFactorForExternalAccess;
            yield return RequireTwoFactorForHighValueTransactions;
            yield return RequireTwoFactorForPasswordChange;
            yield return RequireTwoFactorForEmailChange;
            yield return RequireTwoFactorForPhoneChange;
            yield return RequireTwoFactorForSecuritySettingsChange;
            yield return MaxActiveDevices;
            yield return NotifyOnNewDevice;
            yield return NotifyOnPasswordChange;
            yield return NotifyOnEmailChange;
            yield return NotifyOnPhoneChange;
            yield return RequirePasswordChange;
            yield return PasswordExpiryDate;
        }
    }
} 