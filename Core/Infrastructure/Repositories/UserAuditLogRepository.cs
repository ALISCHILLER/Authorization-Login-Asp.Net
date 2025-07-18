using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserAuditLogRepository : BaseRepository<UserAuditLog>, IUserAuditLogRepository
    {
        private readonly ILogger<UserAuditLogRepository> _logger;

        public UserAuditLogRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<UserAuditLogRepository> logger) : base(context, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserAuditLog>> SearchAsync(
            Guid? userId = null,
            string? action = null,
            string? details = null,
            string? ipAddress = null,
            string? userAgent = null,
            bool? isSuccessful = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Include(ual => ual.User).Where(ual => !ual.IsDeleted);
            if (userId.HasValue) query = query.Where(ual => ual.UserId == userId.Value);
            if (!string.IsNullOrWhiteSpace(action)) query = query.Where(ual => ual.Action == action);
            if (!string.IsNullOrWhiteSpace(details)) query = query.Where(ual => (ual.Details ?? string.Empty).Contains(details));
            if (!string.IsNullOrWhiteSpace(ipAddress)) query = query.Where(ual => ual.IpAddress == ipAddress);
            if (!string.IsNullOrWhiteSpace(userAgent)) query = query.Where(ual => ual.UserAgent == userAgent);
            if (isSuccessful.HasValue) query = query.Where(ual => ual.IsSuccessful == isSuccessful.Value);
            if (startDate.HasValue) query = query.Where(ual => ual.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(ual => ual.CreatedAt <= endDate.Value);
            query = query.OrderByDescending(ual => ual.CreatedAt);
            if (count.HasValue) query = query.Take(count.Value);
            return await query.ToListAsync(cancellationToken);
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
                    // CreatedAt = DateTime.UtcNow // حذف مقداردهی مستقیم چون BaseEntity مقداردهی می‌کند
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

        // متد AddAsync هماهنگ با BaseRepository
        public async Task AddAsync(UserAuditLog log, CancellationToken cancellationToken = default)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log), "لاگ نمی‌تواند خالی باشد");
            await _dbSet.AddAsync(log, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}