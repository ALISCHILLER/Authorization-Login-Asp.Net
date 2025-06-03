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
    public class UserNotificationRepository : BaseRepository<UserNotification>, IUserNotificationRepository
    {
        public UserNotificationRepository(
            ApplicationDbContext context,
            ILogger<UserNotificationRepository> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<UserNotification>> GetByUserAsync(
            Guid userId, 
            bool includeRead = false, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Where(un => un.UserId == userId && !un.IsDeleted);

                if (!includeRead)
                {
                    query = query.Where(un => !un.IsRead);
                }

                return await query
                    .OrderByDescending(un => un.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اعلان‌های کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<UserNotification>> GetByTypeAsync(
            string notificationType, 
            bool includeRead = false, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Include(un => un.User)
                    .Where(un => un.NotificationType == notificationType && !un.IsDeleted);

                if (!includeRead)
                {
                    query = query.Where(un => !un.IsRead);
                }

                return await query
                    .OrderByDescending(un => un.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اعلان‌های با نوع {NotificationType}", notificationType);
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync(
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .CountAsync(un => 
                        un.UserId == userId && 
                        !un.IsRead && 
                        !un.IsDeleted, 
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد اعلان‌های خوانده نشده کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(
            Guid notificationId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _dbSet
                    .FirstOrDefaultAsync(un => 
                        un.Id == notificationId && 
                        !un.IsDeleted, 
                        cancellationToken);

                if (notification == null)
                {
                    return false;
                }

                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در علامت‌گذاری اعلان {NotificationId} به عنوان خوانده شده", notificationId);
                throw;
            }
        }

        public async Task<bool> MarkAllAsReadAsync(
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var unreadNotifications = await _dbSet
                    .Where(un => 
                        un.UserId == userId && 
                        !un.IsRead && 
                        !un.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!unreadNotifications.Any())
                {
                    return true;
                }

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در علامت‌گذاری تمام اعلان‌های کاربر {UserId} به عنوان خوانده شده", userId);
                throw;
            }
        }

        public async Task<UserNotification> AddNotificationAsync(
            Guid userId, 
            string title, 
            string message, 
            string notificationType, 
            string data = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new UserNotification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    Data = data,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(notification, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن اعلان برای کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> DeleteNotificationAsync(
            Guid notificationId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = await _dbSet
                    .FirstOrDefaultAsync(un => 
                        un.Id == notificationId && 
                        !un.IsDeleted, 
                        cancellationToken);

                if (notification == null)
                {
                    return true;
                }

                notification.IsDeleted = true;
                notification.DeletedAt = DateTime.UtcNow;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف اعلان {NotificationId}", notificationId);
                throw;
            }
        }

        public async Task<int> CleanupOldNotificationsAsync(
            TimeSpan age, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.Subtract(age);
                var oldNotifications = await _dbSet
                    .Where(un => 
                        (un.CreatedAt < cutoffTime || un.IsDeleted) && 
                        un.DeletedAt < DateTime.UtcNow.AddDays(-30))
                    .ToListAsync(cancellationToken);

                if (!oldNotifications.Any())
                {
                    return 0;
                }

                _dbSet.RemoveRange(oldNotifications);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی اعلان‌های قدیمی");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetDistinctNotificationTypesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(un => !un.IsDeleted)
                    .Select(un => un.NotificationType)
                    .Distinct()
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست انواع اعلان‌های متمایز");
                throw;
            }
        }

        public async Task<IDictionary<string, int>> GetNotificationTypeStatisticsAsync(
            DateTime? startDate = null, 
            DateTime? endDate = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet.Where(un => !un.IsDeleted);

                if (startDate.HasValue)
                {
                    query = query.Where(un => un.CreatedAt >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(un => un.CreatedAt <= endDate.Value);
                }

                var statistics = await query
                    .GroupBy(un => un.NotificationType)
                    .Select(g => new { NotificationType = g.Key, Count = g.Count() })
                    .ToListAsync(cancellationToken);

                return statistics.ToDictionary(x => x.NotificationType, x => x.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آمار انواع اعلان‌ها");
                throw;
            }
        }
    }
} 