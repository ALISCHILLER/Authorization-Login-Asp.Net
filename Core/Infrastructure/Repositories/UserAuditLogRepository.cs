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
        public UserAuditLogRepository(
            ApplicationDbContext context,
            ILogger<UserAuditLogRepository> logger) : base(context, logger)
        {
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
            string ipAddress = null, 
            string userAgent = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var auditLog = new UserAuditLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
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
    }
} 