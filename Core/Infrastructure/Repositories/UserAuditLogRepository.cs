using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserAuditLogRepository : BaseRepository<UserAuditLog>, IUserAuditLogRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserAuditLogRepository> _logger;
        private string _lastAction = string.Empty;
        private string _lastDetails = string.Empty;
        private string _lastIpAddress = string.Empty;
        private string _lastUserAgent = string.Empty;

        public UserAuditLogRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<UserAuditLogRepository> logger) : base(context, cacheService, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAsync(
            Guid userId, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            string action = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Include(ual => ual.User)
                    .Where(ual => ual.UserId == userId && !ual.IsDeleted);

                if (startDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt <= endDate.Value);
                }

                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(ual => ual.Action == action);
                }

                return await query
                    .OrderByDescending(ual => ual.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لاگ‌های حسابرسی کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserAuditLog>> GetByActionAsync(
            string action, 
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Include(ual => ual.User)
                    .Where(ual => ual.Action == action && !ual.IsDeleted);

                if (startDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt <= endDate.Value);
                }

                return await query
                    .OrderByDescending(ual => ual.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لاگ‌های حسابرسی با عملیات {Action}", action);
                throw;
            }
        }

        public async Task<IEnumerable<UserAuditLog>> GetByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return await _dbSet
                    .Include(ual => ual.User)
                    .Where(ual => 
                        ual.CreatedAt >= startDate && 
                        ual.CreatedAt <= endDate && 
                        !ual.IsDeleted)
                    .OrderByDescending(ual => ual.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لاگ‌های حسابرسی بین {StartDate} و {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<UserAuditLog> AddLogAsync(
            Guid userId, 
            string action, 
            string details, 
            string? ipAddress = null, 
            string? userAgent = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var auditLog = new UserAuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    IpAddress = ipAddress ?? string.Empty,
                    UserAgent = userAgent ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(auditLog, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return auditLog;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن لاگ حسابرسی برای کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<int> CleanupOldLogsAsync(
            TimeSpan age, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.Subtract(age);
                var oldLogs = await _dbSet
                    .Where(ual => ual.CreatedAt < cutoffTime && !ual.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!oldLogs.Any())
                {
                    return 0;
                }

                _dbSet.RemoveRange(oldLogs);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی لاگ‌های حسابرسی قدیمی");
                throw;
            }
        }

        public async Task<IEnumerable<UserAuditLog>> GetByIpAddressAsync(
            string ipAddress, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

            try
            {
                return await _dbSet
                    .Include(ual => ual.User)
                    .Where(ual => 
                        ual.IpAddress == ipAddress && 
                        !ual.IsDeleted)
                    .OrderByDescending(ual => ual.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لاگ‌های حسابرسی با آدرس IP {IpAddress}", ipAddress);
                throw;
            }
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAgentAsync(
            string userAgent, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

            try
            {
                return await _dbSet
                    .Include(ual => ual.User)
                    .Where(ual => 
                        ual.UserAgent == userAgent && 
                        !ual.IsDeleted)
                    .OrderByDescending(ual => ual.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لاگ‌های حسابرسی با User Agent {UserAgent}", userAgent);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetDistinctActionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(ual => !ual.IsDeleted)
                    .Select(ual => ual.Action)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست عملیات‌های متمایز");
                throw;
            }
        }

        public async Task<IDictionary<string, int>> GetActionStatisticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.Where(ual => !ual.IsDeleted);

                if (startDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(ual => ual.CreatedAt <= endDate.Value);
                }

                var statistics = await query
                    .GroupBy(ual => ual.Action)
                    .Select(g => new { Action = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                return statistics.ToDictionary(x => x.Action, x => x.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آمار عملیات‌ها");
                throw;
            }
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _dbSet
                .Where(log => log.UserId == userId && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByActionTypeAsync(string actionType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            return await _dbSet
                .Where(log => log.Action == actionType && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByStatusAsync(bool isSuccessful, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(log => log.IsSuccessful == isSuccessful && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByDetailsAsync(string details, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Details cannot be empty", nameof(details));

            return await _dbSet
                .Where(log => log.Details.Contains(details) && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _dbSet
                .CountAsync(log => log.UserId == userId && !log.IsDeleted, cancellationToken);
        }

        public async Task<int> GetCountByActionTypeAsync(string actionType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            return await _dbSet
                .CountAsync(log => log.Action == actionType && !log.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetLatestLogsAsync(int count, CancellationToken cancellationToken = default)
        {
            if (count < 1)
                throw new ArgumentException("Count must be greater than 0", nameof(count));

            return await _dbSet
                .Where(log => !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAsync(Guid userId, string actionType, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            return await _dbSet
                .Where(log => log.UserId == userId && log.CreatedAt >= startDate && log.CreatedAt <= endDate && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndStatusAsync(Guid userId, bool isSuccessful, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _dbSet
                .Where(log => log.UserId == userId && log.IsSuccessful == isSuccessful && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndDetailsAsync(Guid userId, string details, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Details cannot be empty", nameof(details));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Details.Contains(details) && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndIpAddressAsync(Guid userId, string ipAddress, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

            return await _dbSet
                .Where(log => log.UserId == userId && log.IpAddress == ipAddress && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndUserAgentAsync(Guid userId, string userAgent, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(userAgent))
                throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

            return await _dbSet
                .Where(log => log.UserId == userId && log.UserAgent == userAgent && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAndDateRangeAsync(Guid userId, string actionType, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && log.CreatedAt >= startDate && log.CreatedAt <= endDate && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAndStatusAsync(Guid userId, string actionType, bool isSuccessful, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && log.IsSuccessful == isSuccessful && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAndDetailsAsync(Guid userId, string actionType, string details, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            if (string.IsNullOrWhiteSpace(details))
                throw new ArgumentException("Details cannot be empty", nameof(details));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && log.Details.Contains(details) && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAndIpAddressAsync(Guid userId, string actionType, string ipAddress, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && log.IpAddress == ipAddress && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserAuditLog>> GetByUserAndActionTypeAndUserAgentAsync(Guid userId, string actionType, string userAgent, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(actionType))
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));

            if (string.IsNullOrWhiteSpace(userAgent))
                throw new ArgumentException("User agent cannot be empty", nameof(userAgent));

            return await _dbSet
                .Where(log => log.UserId == userId && log.Action == actionType && log.UserAgent == userAgent && !log.IsDeleted)
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(UserAuditLog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log), "لاگ نمی‌تواند خالی باشد");

            await _context.UserAuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
} 