using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization_Login_Asp.Net.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن تاریخچه ورود
    /// این کلاس عملیات مربوط به تاریخچه ورود کاربران را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class LoginHistoryRepository : Repository<LoginHistory>, ILoginHistoryRepository
    {
        /// <summary>
        /// سازنده کلاس مخزن تاریخچه ورود
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public LoginHistoryRepository(AppDbContext context) : base(context)
        {
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
        public async Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(
            Guid userId, 
            int page = 1, 
            int pageSize = 10,
            bool includeUser = false,
            CancellationToken cancellationToken = default)
        {
            if (page < 1)
                throw new ArgumentException("شماره صفحه نمی‌تواند کمتر از 1 باشد", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("تعداد آیتم در هر صفحه نمی‌تواند کمتر از 1 باشد", nameof(pageSize));

            var query = _dbSet.Where(lh => lh.UserId == userId);

            if (includeUser)
                query = query.Include(lh => lh.User);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(lh => lh.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
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

            var query = _dbSet.Where(lh => lh.LoginTime >= startTime && lh.LoginTime <= endTime);

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

            var query = _dbSet.Where(lh => lh.LoginTime >= startTime && lh.LoginTime <= endTime);

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
    }
} 