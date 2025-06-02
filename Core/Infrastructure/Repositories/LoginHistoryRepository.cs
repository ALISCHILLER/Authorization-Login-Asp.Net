using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن تاریخچه ورود
    /// این کلاس عملیات مربوط به تاریخچه ورود کاربران را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class LoginHistoryRepository : ILoginHistoryRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// سازنده کلاس مخزن تاریخچه ورود
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public LoginHistoryRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<LoginHistory> AddAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            await _context.LoginHistory.AddAsync(loginHistory);
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

            _context.LoginHistory.Remove(loginHistory);
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
            return await _context.LoginHistory.FindAsync(id);
        }

        public async Task<IEnumerable<LoginHistory>> GetAllAsync()
        {
            return await _context.LoginHistory.ToListAsync();
        }

        public async Task<IEnumerable<LoginHistory>> GetAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _context.LoginHistory.Where(predicate).ToListAsync();
        }

        public async Task<LoginHistory> GetFirstOrDefaultAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _context.LoginHistory.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<LoginHistory, bool>> predicate)
        {
            return await _context.LoginHistory.AnyAsync(predicate);
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
            return await _context.LoginHistory
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
            return _context.LoginHistory.Where(lh => lh.UserId == userId);
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

            return await _context.LoginHistory
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
            var query = _context.LoginHistory.Where(lh => lh.UserId == userId);
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

            var query = _context.LoginHistory
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

            var query = _context.LoginHistory
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

            return await _context.LoginHistory
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

            return await _context.LoginHistory
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

            var sessions = await _context.LoginHistory
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

            return await _context.LoginHistory
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