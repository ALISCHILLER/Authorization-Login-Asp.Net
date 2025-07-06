using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Events;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class User : AggregateRoot // حذف IAuditable برای رفع تداخل پراپرتی‌ها
    {
        private readonly List<UserRole> _userRoles = new();
        private readonly List<UserDevice> _userDevices = new();
        private readonly List<RefreshToken> _refreshTokens = new();
        private readonly List<TwoFactorRecoveryCode> _recoveryCodes = new();
        private readonly List<LoginHistory> _loginHistory = new();
        private readonly List<UserSession> _sessions = new();
        private readonly List<AuditLog> _auditLogs = new();
        private readonly List<Notification> _notifications = new();

        private User() { 
            Username = string.Empty;
            Email = new Email("");
            PasswordHash = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            PhoneNumber = string.Empty;
            FullName = string.Empty;
            SecurityStamp = string.Empty;
            SecuritySettings = new UserSecuritySettings();
            ProfileImageUrl = string.Empty;
            TwoFactorSecret = string.Empty;
            UpdatedBy = string.Empty;
        } // For EF Core

        public User(
            string username,
            string email,
            string passwordHash,
            string firstName,
            string lastName,
            string phoneNumber)
        {
            Username = username ?? string.Empty;
            Email = new Email(email ?? string.Empty);
            PasswordHash = passwordHash ?? string.Empty;
            FirstName = firstName ?? string.Empty;
            LastName = lastName ?? string.Empty;
            PhoneNumber = phoneNumber ?? string.Empty;
            FullName = $"{FirstName} {LastName}";
            SecurityStamp = Guid.NewGuid().ToString("N");
            SecuritySettings = new UserSecuritySettings();
            ProfileImageUrl = "/images/default-profile.png";
            TwoFactorSecret = string.Empty;
            UpdatedBy = string.Empty;
            AddDomainEvent(new UserCreatedEvent(Id));
            // Initialize PasswordSalt, though it will be set by Application layer
            PasswordSalt = string.Empty;
        }

        [Required, MaxLength(50)]
        public string Username { get; set; }
        public string NormalizedUserName => Username?.ToUpperInvariant() ?? string.Empty;
        public string NormalizedEmail => Email?.Value?.ToUpperInvariant() ?? string.Empty;
        public string SecurityStamp { get; set; }

        [Required]
        public Email Email { get; set; }
        
        public string EmailAddress
        {
            get => Email?.Value ?? string.Empty;
            private set => Email = new Email(value);
        }

        [Required, MaxLength(256)] // Increased length for potentially longer hashes
        public string PasswordHash { get; set; }

        [Required, MaxLength(128)] // Added for storing salt
        public string PasswordSalt { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; }

        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; }

        [MaxLength(500)]
        public string ProfileImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? AccountLockoutEnd { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public TwoFactorType? TwoFactorType { get; set; }
        public string? TwoFactorSecret { get; set; }
        public UserSecuritySettings SecuritySettings { get; set; }

        // Removed: public string UpdatedBy { get; set; } = string.Empty;
        // UpdatedBy is inherited from BaseEntity as string?

        public string? RefreshToken { get; set; }
        public string? VerificationToken { get; set; }
        public string? PasswordResetToken { get; set; }

        // Removed: public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // CreatedAt is inherited from BaseEntity and set in its constructor.

        // Removed: public DateTime? UpdatedAt { get; set; }
        // UpdatedAt is inherited from BaseEntity as DateTime?

        // Navigation property for Roles
        public ICollection<Role> Roles { get; set; } = new List<Role>();

        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        public virtual IReadOnlyCollection<UserDevice> UserDevices => _userDevices.AsReadOnly();
        public virtual IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        public virtual IReadOnlyCollection<TwoFactorRecoveryCode> RecoveryCodes => _recoveryCodes.AsReadOnly();
        public virtual IReadOnlyCollection<LoginHistory> LoginHistory => _loginHistory.AsReadOnly();
        public virtual IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();
        public virtual IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();
        public virtual IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        public void UpdateProfile(string firstName, string lastName, string phoneNumber)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            FullName = $"{FirstName} {LastName}";

            AddDomainEvent(new UserUpdatedEvent(Id));
        }

        public void UpdateEmail(string email)
        {
            Email = new Email(email) ?? throw new ArgumentNullException(nameof(email));
            IsEmailVerified = false;

            AddDomainEvent(new UserEmailChangedEvent(Id, email));
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            AddDomainEvent(new UserEmailVerifiedEvent(Id));
        }

        public void ChangePassword(string newPasswordHash, string newPasswordSalt)
        {
            PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
            PasswordSalt = newPasswordSalt ?? throw new ArgumentNullException(nameof(newPasswordSalt));
            LastPasswordChange = DateTime.UtcNow;
            SecurityStamp = Guid.NewGuid().ToString("N"); // Good practice to change security stamp on password change

            AddDomainEvent(new UserPasswordChangedEvent(Id));
        }

        public void EnableTwoFactor(TwoFactorType type, string secret)
        {
            TwoFactorEnabled = true;
            TwoFactorType = type;
            TwoFactorSecret = secret;

            AddDomainEvent(new UserTwoFactorEnabledEvent(Id));
        }

        public void DisableTwoFactor()
        {
            TwoFactorEnabled = false;
            TwoFactorType = null;
            TwoFactorSecret = null;

            AddDomainEvent(new UserTwoFactorDisabledEvent(Id));
        }

        public void RecordLoginAttempt(bool successful)
        {
            if (successful)
            {
                LastLoginAt = DateTime.UtcNow;
                FailedLoginAttempts = 0;
                AccountLockoutEnd = null;
                // AddDomainEvent(new UserLoggedInEvent(Id)); // حذف چون ایونت وجود ندارد
            }
            else
            {
                FailedLoginAttempts++;
                if (FailedLoginAttempts >= SecuritySettings.MaxFailedLoginAttempts)
                {
                    AccountLockoutEnd = DateTime.UtcNow.AddMinutes(15); // مقدار پیش‌فرض
                    // AddDomainEvent(new UserAccountLockedEvent(Id)); // حذف چون ایونت وجود ندارد
                }
            }
        }

        public void UpdateProfileImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));
            ProfileImageUrl = imageUrl;
            // AddDomainEvent(new UserProfileImageUpdatedEvent(Id, imageUrl)); // حذف چون ایونت وجود ندارد
        }

        public void AddRole(UserRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            _userRoles.Add(role);
            // AddDomainEvent(new UserRoleAddedEvent(Id, role.RoleId)); // حذف چون ایونت وجود ندارد
        }

        public void RemoveRole(UserRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            _userRoles.Remove(role);
            // AddDomainEvent(new UserRoleRemovedEvent(Id, role.RoleId)); // حذف چون ایونت وجود ندارد
        }

        public void AddDevice(UserDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            _userDevices.Add(device);
            // AddDomainEvent(new UserDeviceAddedEvent(Id, device.DeviceId)); // حذف چون ایونت وجود ندارد
        }

        public void AddRefreshToken(RefreshToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            _refreshTokens.Add(token);
        }

        public void AddLoginHistory(LoginHistory login)
        {
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            _loginHistory.Add(login);
        }

        // Static Create method that directly hashes password is removed.
        // Password hashing and salt generation should be handled by the Application layer using IPasswordHasher.
        // The constructor User(...) already accepts passwordHash. A new constructor or factory method
        // in the application layer should be used.

        // Instance method VerifyPassword that directly uses BCrypt is removed.
        // Password verification should be handled by the Application layer using IPasswordHasher.

        public void IncrementFailedLoginAttempts()
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= 5)
                AccountLockoutEnd = DateTime.UtcNow.AddMinutes(15);
        }

        public void ResetFailedLoginAttempts()
        {
            FailedLoginAttempts = 0;
            AccountLockoutEnd = null;
        }

        public bool IsAccountLocked()
        {
            return AccountLockoutEnd.HasValue && AccountLockoutEnd.Value > DateTime.UtcNow;
        }

        public List<string> BackupCodes { get; set; } = new List<string>();

        // Using DeleteAuditable and RestoreAuditable from AggregateRoot (which call base methods and add events)
        // These overrides ensure IsActive is also managed.
        public void DeleteUser(string? deletedByUserId = null)
        {
            if (!IsDeleted)
            {
                base.DeleteAuditable(deletedByUserId); // Calls base.MarkAsDeleted and adds EntityDeletedEvent
                IsActive = false;
                // If a more specific UserDeletedEvent is needed, it can be added here too,
                // but EntityDeletedEvent from AggregateRoot already covers the deletion.
            }
        }

        public void RestoreUser(string? restoredByUserId = null)
        {
            if (IsDeleted)
            {
                base.RestoreAuditable(restoredByUserId); // Calls base.MarkAsRestored and adds EntityRestoredEvent
                IsActive = true;
                // Similarly, a UserRestoredEvent could be added if needed.
            }
        }
    }
}