using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن تاریخچه ورود
    /// این کلاس عملیات مربوط به تاریخچه ورود کاربران را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class LoginHistoryRepository : BaseRepository<LoginHistory>, ILoginHistoryRepository
    {
        public LoginHistoryRepository(
            ApplicationDbContext context,
            ILogger<LoginHistoryRepository> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// دریافت تاریخچه ورود کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="limit">تعداد رکوردهای مورد نظر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست تاریخچه ورود کاربر</returns>
        public async Task<IEnumerable<LoginHistory>> GetByUserAsync(
            Guid userId, 
            int? limit = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Where(lh => lh.UserId == userId && !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime);

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                return await query.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود کاربر {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه ورود کاربر در بازه زمانی مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startDate">تاریخ شروع</param>
        /// <param name="endDate">تاریخ پایان</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست تاریخچه ورود کاربر در بازه زمانی</returns>
        public async Task<IEnumerable<LoginHistory>> GetByUserAndDateRangeAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(lh => lh.UserId == userId && 
                                !lh.IsDeleted && 
                                lh.LoginTime >= startDate && 
                                lh.LoginTime <= endDate)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود کاربر {UserId} در بازه زمانی", userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تاریخچه ورود کاربر با دستگاه مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="deviceId">شناسه دستگاه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست تاریخچه ورود کاربر با دستگاه مشخص</returns>
        public async Task<IEnumerable<LoginHistory>> GetByUserAndDeviceAsync(
            Guid userId,
            Guid deviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(lh => lh.UserId == userId && 
                                lh.DeviceId == deviceId && 
                                !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود کاربر {UserId} با دستگاه {DeviceId}", userId, deviceId);
                throw;
            }
        }

        /// <summary>
        /// دریافت آخرین ورود کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>آخرین رکورد ورود کاربر</returns>
        public async Task<LoginHistory> GetLastLoginAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(lh => lh.UserId == userId && !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت آخرین ورود کاربر {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد ورودهای ناموفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="timeWindow">بازه زمانی (به دقیقه)</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد ورودهای ناموفق</returns>
        public async Task<int> GetFailedLoginCountAsync(
            Guid userId,
            int timeWindow = 30,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-timeWindow);
                return await _dbSet
                    .CountAsync(lh => lh.UserId == userId && 
                                     !lh.IsDeleted && 
                                     !lh.IsSuccessful && 
                                     lh.LoginTime >= cutoffTime,
                               cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تعداد ورودهای ناموفق کاربر {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// پاکسازی تاریخچه ورودهای قدیمی
        /// </summary>
        /// <param name="olderThan">تاریخ مرجع برای حذف</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد رکوردهای حذف شده</returns>
        public async Task<int> CleanupOldLoginHistoryAsync(
            DateTime olderThan,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var oldRecords = await _dbSet
                    .Where(lh => lh.LoginTime < olderThan && !lh.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!oldRecords.Any())
                    return 0;

                foreach (var record in oldRecords)
                {
                    record.IsDeleted = true;
                    record.DeletedAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی تاریخچه ورودهای قدیمی");
                throw;
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(lh => lh.User)
                    .Where(lh => 
                        lh.LoginTime >= startDate && 
                        lh.LoginTime <= endDate && 
                        !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود در بازه زمانی {StartDate} تا {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetFailedLoginsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _dbSet
                    .Include(lh => lh.User)
                    .Where(lh => !lh.IsSuccessful && !lh.IsDeleted);

                if (startDate.HasValue)
                {
                    query = query.Where(lh => lh.LoginTime >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(lh => lh.LoginTime <= endDate.Value);
                }

                return await query
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت ورودهای ناموفق");
                throw;
            }
        }

        public async Task<bool> HasRecentFailedLoginsAsync(
            Guid userId,
            int maxAttempts,
            TimeSpan timeWindow,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.Subtract(timeWindow);
                return await _dbSet
                    .CountAsync(lh => 
                        lh.UserId == userId && 
                        !lh.IsSuccessful && 
                        !lh.IsDeleted && 
                        lh.LoginTime >= cutoffTime, 
                        cancellationToken) >= maxAttempts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی ورودهای ناموفق اخیر کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<int> CleanupOldHistoryAsync(
            int daysToKeep,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldRecords = await _dbSet
                    .Where(lh => lh.LoginTime < cutoffDate && !lh.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!oldRecords.Any())
                {
                    return 0;
                }

                foreach (var record in oldRecords)
                {
                    record.IsDeleted = true;
                    record.DeletedAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی تاریخچه ورود قدیمی");
                throw;
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetByIpAddressAsync(
            string ipAddress,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(lh => lh.User)
                    .Where(lh => 
                        lh.IpAddress == ipAddress && 
                        !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود با آدرس IP {IpAddress}", ipAddress);
                throw;
            }
        }

        public async Task<IEnumerable<LoginHistory>> GetByDeviceInfoAsync(
            string deviceInfo,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(lh => lh.User)
                    .Where(lh => 
                        lh.DeviceInfo == deviceInfo && 
                        !lh.IsDeleted)
                    .OrderByDescending(lh => lh.LoginTime)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت تاریخچه ورود با اطلاعات دستگاه {DeviceInfo}", deviceInfo);
                throw;
            }
        }

        public async Task<LoginHistory> AddAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            await _dbSet.AddAsync(loginHistory);
            return loginHistory;
        }

        Task ILoginHistoryRepository.AddAsync(LoginHistory loginHistory)
        {
            return AddAsync(loginHistory).ContinueWith(t => Task.CompletedTask);
        }

        public async Task<bool> RemoveAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            _dbSet.Remove(loginHistory);
            return await SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            _context.Entry(loginHistory).State = EntityState.Modified;
            return await SaveChangesAsync() > 0;
        }

        public async Task<LoginHistory> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<LoginHistory>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<LoginHistory>> GetAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<LoginHistory> GetFirstOrDefaultAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
            catch (DbUpdateException)
            {
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
        }

        Task<bool> IRepository<LoginHistory>.SaveChangesAsync()
        {
            return SaveChangesAsync().ContinueWith(t => Task.FromResult(t.Result > 0));
        }

        /// <summary>
        /// دریافت آخرین ورود موفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>آخرین رکورد ورود موفق کاربر</returns>
        public async Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(lh => lh.UserId == userId && lh.IsSuccessful)
                .OrderByDescending(lh => lh.LoginTime)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت کوئری تاریخچه ورود کاربر برای استفاده در عملیات‌های پیچیده‌تر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>کوئری تاریخچه ورود کاربر</returns>
        public IQueryable<LoginHistory> GetLoginHistoryQuery(Guid userId)
        {
            return _dbSet.Where(lh => lh.UserId == userId);
        }

        /// <summary>
        /// دریافت تعداد تلاش‌های ناموفق ورود کاربر در بازه زمانی مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد تلاش‌های ناموفق</returns>
        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            return await _dbSet
                .CountAsync(lh =>
                    lh.UserId == userId &&
                    !lh.IsSuccessful &&
                    lh.LoginTime >= startTime &&
                    lh.LoginTime <= endTime,
                    cancellationToken);
        }

        /// <summary>
        /// دریافت تاریخچه ورود کاربر با امکان صفحه‌بندی و مرتب‌سازی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <param name="includeUser">آیا اطلاعات کاربر نیز باید بارگذاری شود؟</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تاپل شامل لیست تاریخچه ورود و تعداد کل رکوردها</returns>
        public async Task<LoginHistoryResult> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            var query = _dbSet.Where(lh => lh.UserId == userId);
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(lh => lh.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new LoginHistoryResult(items, totalCount);
        }

        /// <summary>
        /// دریافت تاریخچه ورود کاربران در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="includeUser">آیا اطلاعات کاربر نیز باید بارگذاری شود؟</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست تاریخچه ورود کاربران در بازه زمانی مشخص</returns>
        public async Task<IEnumerable<LoginHistory>> GetLoginHistoryByDateRangeAsync(
            DateTime startTime,
            DateTime endTime,
            bool includeUser = false,
            CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            var query = _dbSet
                .Where(lh => lh.LoginTime >= startTime && lh.LoginTime <= endTime);

            if (includeUser)
                query = query.Include(lh => lh.User);

            return await query
                .OrderByDescending(lh => lh.LoginTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت آمار ورود کاربران در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تاپل شامل تعداد کل ورودها، تعداد ورودهای موفق و تعداد ورودهای ناموفق</returns>
        public async Task<(int TotalLogins, int SuccessfulLogins, int FailedLogins)> GetLoginStatisticsAsync(
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            var query = _dbSet
                .Where(lh => lh.LoginTime >= startTime && lh.LoginTime <= endTime);

            var totalLogins = await query.CountAsync(cancellationToken);
            var successfulLogins = await query.CountAsync(lh => lh.IsSuccessful, cancellationToken);
            var failedLogins = totalLogins - successfulLogins;

            return (totalLogins, successfulLogins, failedLogins);
        }

        /// <summary>
        /// دریافت تاریخچه ورودهای ناموفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="count">تعداد رکوردهای مورد نیاز</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست آخرین ورودهای ناموفق کاربر</returns>
        public async Task<IEnumerable<LoginHistory>> GetRecentFailedLoginsAsync(
            Guid userId,
            int count = 5,
            CancellationToken cancellationToken = default)
        {
            if (count < 1)
                throw new ArgumentException("تعداد رکوردها نمی‌تواند کمتر از 1 باشد", nameof(count));

            return await _dbSet
                .Where(lh => lh.UserId == userId && !lh.IsSuccessful)
                .OrderByDescending(lh => lh.LoginTime)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی وجود ورود موفق کاربر در بازه زمانی مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر در بازه زمانی مشخص ورود موفق داشته باشد</returns>
        public async Task<bool> HasSuccessfulLoginInRangeAsync(
            Guid userId,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            return await _dbSet
                .AnyAsync(lh =>
                    lh.UserId == userId &&
                    lh.IsSuccessful &&
                    lh.LoginTime >= startTime &&
                    lh.LoginTime <= endTime,
                    cancellationToken);
        }

        /// <summary>
        /// دریافت مدت زمان حضور کاربر در سیستم
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>مجموع مدت زمان حضور کاربر در سیستم (به دقیقه)</returns>
        public async Task<int> GetTotalSessionDurationAsync(
            Guid userId,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            var sessions = await _dbSet
                .Where(lh =>
                    lh.UserId == userId &&
                    lh.IsSuccessful &&
                    lh.LoginTime >= startTime &&
                    lh.LoginTime <= endTime &&
                    lh.LogoutTime.HasValue)
                .Select(lh => (int)(lh.LogoutTime.Value - lh.LoginTime).TotalMinutes)
                .ToListAsync(cancellationToken);

            return sessions.Sum();
        }

        /// <summary>
        /// دریافت تعداد کاربران فعال در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد کاربران فعال</returns>
        public async Task<int> GetActiveUsersCountAsync(
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default)
        {
            if (startTime > endTime)
                throw new ArgumentException("زمان شروع نمی‌تواند بعد از زمان پایان باشد", nameof(startTime));

            return await _dbSet
                .Where(lh =>
                    lh.IsSuccessful &&
                    lh.LoginTime >= startTime &&
                    lh.LoginTime <= endTime)
                .Select(lh => lh.UserId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

        Task<LoginHistoryResult> ILoginHistoryRepository.GetUserLoginHistoryAsync(Guid userId, int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}