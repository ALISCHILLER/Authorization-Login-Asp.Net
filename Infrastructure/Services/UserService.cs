using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionService _permissionService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly IJwtService _jwtService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPermissionService permissionService,
            IEmailService emailService,
            ISmsService smsService,
            IJwtService jwtService,
            IRefreshTokenService refreshTokenService,
            ILogger<UserService> logger,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _permissionService = permissionService;
            _emailService = emailService;
            _smsService = smsService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {id}");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by id {UserId}", id);
                throw new UserServiceException("Failed to get user", ex);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with email {email}");
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by email {Email}", email);
                throw new UserServiceException("Failed to get user", ex);
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _userRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users");
                throw new UserServiceException("Failed to get users", ex);
            }
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            try
            {
                // Validate email uniqueness
                var existingUser = await _userRepository.GetByEmailAsync(user.Email.Value);
                if (existingUser != null)
                {
                    throw new UserServiceException($"Email {user.Email.Value} is already registered");
                }

                // Hash password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsEmailVerified = false;
                user.TwoFactorEnabled = false;

                // Create user
                await _userRepository.AddAsync(user);

                // Send verification email
                var verificationToken = Guid.NewGuid().ToString();
                var verificationLink = $"{_configuration["AppSettings:BaseUrl"]}/verify-email?token={verificationToken}";
                await _emailService.SendVerificationEmailAsync(user.Email.Value, verificationLink);

                _logger.LogInformation("Created new user with email {Email}", user.Email.Value);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user with email {Email}", user.Email.Value);
                throw new UserServiceException("Failed to create user", ex);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new UserServiceException($"User not found with id {user.Id}");
                }

                // Preserve sensitive data
                user.PasswordHash = existingUser.PasswordHash;
                user.IsEmailVerified = existingUser.IsEmailVerified;
                user.TwoFactorEnabled = existingUser.TwoFactorEnabled;
                user.CreatedAt = existingUser.CreatedAt;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Updated user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", user.Id);
                throw new UserServiceException("Failed to update user", ex);
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {id}");
                }

                await _userRepository.DeleteAsync(id);
                _logger.LogInformation("Deleted user {UserId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                throw new UserServiceException("Failed to delete user", ex);
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate credentials for email {Email}", email);
                throw new UserServiceException("Failed to validate credentials", ex);
            }
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    throw new UserServiceException("Current password is incorrect");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);

                // Send password change notification
                await _emailService.SendEmailAsync(
                    user.Email.Value,
                    "Password Changed",
                    "Your password has been changed successfully. If you did not make this change, please contact support immediately."
                );

                _logger.LogInformation("Changed password for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to change password for user {UserId}", userId);
                throw new UserServiceException("Failed to change password", ex);
            }
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the email doesn't exist
                    return;
                }

                var resetToken = Guid.NewGuid().ToString();
                var resetLink = $"{_configuration["ApplicationSettings:BaseUrl"]}/reset-password?token={resetToken}";
                await _emailService.SendPasswordResetEmailAsync(email, resetLink);

                _logger.LogInformation("Sent password reset email to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw new UserServiceException("Failed to send password reset email", ex);
            }
        }

        public async Task EnableTwoFactorAsync(Guid userId, string phoneNumber)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                user.TwoFactorEnabled = true;
                user.PhoneNumber = phoneNumber;
                user.TwoFactorType = TwoFactorType.Sms;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Enabled two-factor authentication for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enable two-factor authentication for user {UserId}", userId);
                throw new UserServiceException("Failed to enable two-factor authentication", ex);
            }
        }

        public async Task DisableTwoFactorAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                user.TwoFactorEnabled = false;
                user.TwoFactorType = null;
                user.TwoFactorSecret = null;

                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Disabled two-factor authentication for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disable two-factor authentication for user {UserId}", userId);
                throw new UserServiceException("Failed to disable two-factor authentication", ex);
            }
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                if (!user.TwoFactorEnabled)
                {
                    throw new UserServiceException("Two-factor authentication is not enabled for this user");
                }

                // Verify code based on two-factor type
                switch (user.TwoFactorType)
                {
                    case TwoFactorType.Sms:
                        return await _smsService.VerifyCodeAsync(user.PhoneNumber, code);
                    case TwoFactorType.Email:
                        return await _emailService.VerifyCodeAsync(user.Email.Value, code);
                    case TwoFactorType.Authenticator:
                        return await _jwtService.VerifyAuthenticatorCodeAsync(user.TwoFactorSecret, code);
                    default:
                        throw new UserServiceException("Invalid two-factor authentication type");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify two-factor code for user {UserId}", userId);
                throw new UserServiceException("Failed to verify two-factor code", ex);
            }
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email.Value),
                    new Claim("email_verified", user.IsEmailVerified.ToString()),
                    new Claim("two_factor_enabled", user.TwoFactorEnabled.ToString())
                };

                // Add role claims
                var role = await _roleRepository.GetByIdAsync(user.Role);
                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    var permissions = await _permissionService.GetPermissionsByRoleAsync(role.Id);
                    foreach (var permission in permissions)
                    {
                        claims.Add(new Claim("permission", permission.Name));
                    }
                }

                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get claims for user {UserId}", userId);
                throw new UserServiceException("Failed to get user claims", ex);
            }
        }

        public async Task AddUserDeviceAsync(Guid userId, UserDevice device)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                device.UserId = userId;
                device.CreatedAt = DateTime.UtcNow;
                device.LastUsedAt = DateTime.UtcNow;
                device.IsActive = true;

                user.ConnectedDevices.Add(device);
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Added new device for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add device for user {UserId}", userId);
                throw new UserServiceException("Failed to add device", ex);
            }
        }

        public async Task RemoveUserDeviceAsync(Guid userId, Guid deviceId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                var device = user.ConnectedDevices.FirstOrDefault(d => d.Id == deviceId);
                if (device == null)
                {
                    throw new UserServiceException($"Device not found with id {deviceId}");
                }

                device.IsActive = false;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Removed device {DeviceId} for user {UserId}", deviceId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove device {DeviceId} for user {UserId}", deviceId, userId);
                throw new UserServiceException("Failed to remove device", ex);
            }
        }

        public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new UserServiceException($"User not found with id {userId}");
                }

                return user.ConnectedDevices.Where(d => d.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get devices for user {UserId}", userId);
                throw new UserServiceException("Failed to get user devices", ex);
            }
        }
    }

    public class UserServiceException : Exception
    {
        public UserServiceException(string message) : base(message) { }
        public UserServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
