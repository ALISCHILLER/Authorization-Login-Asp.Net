using Authorization_Login_Asp.Net.Core.Application.Interfaces; // For ILoginHistoryService
using Authorization_Login_Asp.Net.Core.Domain.Interfaces; // For IUserRepository or a dedicated ILoginHistoryRepository
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities; // For LoginHistory entity
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth; // For DeviceInfo

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly IUserRepository _userRepository; // Or ILoginHistoryRepository if it exists
        private readonly ILogger<LoginHistoryService> _logger;
        private readonly IDateTimeService _dateTimeService; // Added

        public LoginHistoryService(
            IUserRepository userRepository,
            ILogger<LoginHistoryService> logger,
            IDateTimeService dateTimeService) // Added
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService)); // Added
        }

        // Methods from ILoginHistoryService, moved from AuthenticationService.cs

        public async Task LogSuccessfulLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent ?? string.Empty,
                DeviceName = deviceInfo?.DeviceName ?? string.Empty,
                DeviceType = deviceInfo?.DeviceType ?? string.Empty,
                OperatingSystem = deviceInfo?.OperatingSystem ?? string.Empty,
                Browser = deviceInfo?.BrowserName ?? string.Empty,
                IsSuccessful = true,
                LoginTime = _dateTimeService.UtcNow
            };
            await _userRepository.AddLoginHistoryAsync(loginHistory);
            // TODO: Review SaveChangesAsync strategy. Currently assuming UnitOfWork pattern.
            // If not, IUserRepository might need a SaveChangesAsync method or it should be called here.
             _logger.LogInformation("Successful login logged for User ID: {UserId}", userId);
        }

        public async Task LogFailedLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo, string reason)
        {
            var loginHistory = new LoginHistory
            {
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent ?? string.Empty,
                DeviceName = deviceInfo?.DeviceName ?? string.Empty,
                DeviceType = deviceInfo?.DeviceType ?? string.Empty,
                OperatingSystem = deviceInfo?.OperatingSystem ?? string.Empty,
                Browser = deviceInfo?.BrowserName ?? string.Empty,
                IsSuccessful = false,
                FailureReason = reason,
                LoginTime = _dateTimeService.UtcNow
            };
            await _userRepository.AddLoginHistoryAsync(loginHistory);
            // TODO: Review SaveChangesAsync strategy.
            _logger.LogWarning("Failed login attempt logged for User ID: {UserId}, Reason: {Reason}", userId, reason);
        }

        public async Task LogLogoutAsync(Guid userId)
        {
            var lastLogin = await _userRepository.GetLastLoginHistoryAsync(userId);
            if (lastLogin != null && !lastLogin.LogoutTime.HasValue)
            {
                lastLogin.LogoutTime = _dateTimeService.UtcNow;
                if (lastLogin.LoginTime != DateTime.MinValue) // Ensure LoginTime is valid
                {
                    lastLogin.SessionDuration = (int)(lastLogin.LogoutTime.Value - lastLogin.LoginTime).TotalSeconds;
                }
                await _userRepository.UpdateLoginHistoryAsync(lastLogin);
                // TODO: Review SaveChangesAsync strategy.
                _logger.LogInformation("Logout logged for User ID: {UserId}", userId);
            }
        }

        public async Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var items = await _userRepository.GetLoginHistoryAsync(userId, page, pageSize);
            var totalCount = await _userRepository.GetLoginHistoryCountAsync(userId);
            return (items.ToList(), totalCount);
        }

        public async Task<LoginHistory?> GetLastSuccessfulLoginAsync(Guid userId)
        {
            return await _userRepository.GetLastSuccessfulLoginAsync(userId);
        }

        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15)
        {
            return await _userRepository.GetFailedLoginAttemptsCountAsync(userId, timeWindowMinutes);
        }

        public async Task RecordLoginAsync(Guid userId, string ipAddress, string? deviceToken, CancellationToken cancellationToken = default)
        {
            // Assuming deviceToken can be part of UserAgent or a new field in DeviceInfo if necessary.
            // For simplicity, passing deviceToken as part of userAgent or as a device name if deviceInfo is null.
            var deviceInfo = new DeviceInfo { DeviceName = deviceToken ?? "Unknown" }; // Simplified
            await LogSuccessfulLoginAsync(userId, ipAddress, deviceToken, deviceInfo);
            _logger.LogInformation("Login recorded via RecordLoginAsync for User ID: {UserId}", userId);
        }
    }
}
