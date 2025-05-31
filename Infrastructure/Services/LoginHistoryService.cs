using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت تاریخچه ورود کاربران
    /// </summary>
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly ILogger<LoginHistoryService> _logger;
        private readonly IUserRepository _userRepository;

        public LoginHistoryService(
            ILogger<LoginHistoryService> logger,
            IUserRepository userRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <inheritdoc/>
        public async Task<LoginHistory> LogSuccessfulLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo)
        {
            try
            {
                var loginHistory = new LoginHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceType = deviceInfo.DeviceType,
                    OperatingSystem = deviceInfo.OperatingSystem,
                    BrowserName = deviceInfo.BrowserName,
                    BrowserVersion = deviceInfo.BrowserVersion,
                    Country = deviceInfo.Country,
                    City = deviceInfo.City,
                    IsSuccessful = true
                };

                await _userRepository.AddLoginHistoryAsync(loginHistory);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("ورود موفق کاربر {UserId} ثبت شد", userId);
                return loginHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت ورود موفق کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در ثبت ورود موفق", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<LoginHistory> LogFailedLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo, string failureReason)
        {
            try
            {
                var loginHistory = new LoginHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceType = deviceInfo.DeviceType,
                    OperatingSystem = deviceInfo.OperatingSystem,
                    BrowserName = deviceInfo.BrowserName,
                    BrowserVersion = deviceInfo.BrowserVersion,
                    Country = deviceInfo.Country,
                    City = deviceInfo.City,
                    IsSuccessful = false,
                    FailureReason = failureReason
                };

                await _userRepository.AddLoginHistoryAsync(loginHistory);
                await _userRepository.SaveChangesAsync();

                _logger.LogWarning("ورود ناموفق کاربر {UserId} ثبت شد. دلیل: {Reason}", userId, failureReason);
                return loginHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت ورود ناموفق کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در ثبت ورود ناموفق", ex);
            }
        }

        /// <inheritdoc/>
        public async Task LogLogoutAsync(Guid userId)
        {
            try
            {
                var lastLogin = await _userRepository.GetLastSuccessfulLoginAsync(userId);
                if (lastLogin != null)
                {
                    lastLogin.Logout();
                    await _userRepository.UpdateLoginHistoryAsync(lastLogin);
                    await _userRepository.SaveChangesAsync();

                    _logger.LogInformation("خروج کاربر {UserId} ثبت شد", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت خروج کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در ثبت خروج", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _userRepository.GetLoginHistoryQuery(userId);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(x => x.LoginTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در دریافت تاریخچه ورود", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId)
        {
            try
            {
                var lastLogin = await _userRepository.GetLastSuccessfulLoginAsync(userId);
                if (lastLogin == null)
                {
                    _logger.LogWarning("ورود موفق قبلی برای کاربر {UserId} یافت نشد", userId);
                }
                return lastLogin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آخرین ورود موفق کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در دریافت آخرین ورود موفق", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeWindowMinutes);
                return await _userRepository.GetLoginHistoryQuery(userId)
                    .Where(x => !x.IsSuccessful && x.LoginTime >= cutoffTime)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد تلاش‌های ناموفق ورود کاربر {UserId}", userId);
                throw new LoginHistoryException("خطا در دریافت تعداد تلاش‌های ناموفق ورود", ex);
            }
        }
    }

    /// <summary>
    /// استثنای مربوط به تاریخچه ورود
    /// </summary>
    public class LoginHistoryException : Exception
    {
        public LoginHistoryException(string message) : base(message) { }
        public LoginHistoryException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
} 