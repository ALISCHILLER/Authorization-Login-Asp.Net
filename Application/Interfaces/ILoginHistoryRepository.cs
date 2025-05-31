using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس ریپازیتوری تاریخچه ورود؛ این کلاس متدهای مربوط به مدیریت تاریخچه ورود کاربران را تعریف می‌کند.
    /// </summary>
    public interface ILoginHistoryRepository : IRepository<LoginHistory>
    {
        /// <summary>
        /// دریافت آخرین ورود موفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>آخرین رکورد ورود موفق کاربر</returns>
        Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کوئری تاریخچه ورود کاربر برای استفاده در عملیات‌های پیچیده‌تر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>کوئری تاریخچه ورود کاربر</returns>
        IQueryable<LoginHistory> GetLoginHistoryQuery(Guid userId);

        /// <summary>
        /// دریافت تعداد تلاش‌های ناموفق ورود کاربر در بازه زمانی مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد تلاش‌های ناموفق</returns>
        Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تاریخچه ورود کاربر با صفحه‌بندی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <param name="includeUser">آیا اطلاعات کاربر نیز بازگردانده شود</param>
        /// <returns>لیست تاریخچه ورود و تعداد کل رکوردها</returns>
        Task<LoginHistoryResult> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// دریافت تاریخچه ورود کاربران در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="includeUser">آیا اطلاعات کاربر نیز باید بارگذاری شود؟</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست تاریخچه ورود کاربران در بازه زمانی مشخص</returns>
        Task<IEnumerable<LoginHistory>> GetLoginHistoryByDateRangeAsync(
            DateTime startTime, 
            DateTime endTime,
            bool includeUser = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت آمار ورود کاربران در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تاپل شامل تعداد کل ورودها، تعداد ورودهای موفق و تعداد ورودهای ناموفق</returns>
        Task<(int TotalLogins, int SuccessfulLogins, int FailedLogins)> GetLoginStatisticsAsync(
            DateTime startTime, 
            DateTime endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تاریخچه ورودهای ناموفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="count">تعداد رکوردهای مورد نیاز</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست آخرین ورودهای ناموفق کاربر</returns>
        Task<IEnumerable<LoginHistory>> GetRecentFailedLoginsAsync(
            Guid userId,
            int count = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود ورود موفق کاربر در بازه زمانی مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر در بازه زمانی مشخص ورود موفق داشته باشد</returns>
        Task<bool> HasSuccessfulLoginInRangeAsync(
            Guid userId,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت مدت زمان حضور کاربر در سیستم
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>مجموع مدت زمان حضور کاربر در سیستم (به دقیقه)</returns>
        Task<int> GetTotalSessionDurationAsync(
            Guid userId,
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تعداد کاربران فعال در بازه زمانی مشخص
        /// </summary>
        /// <param name="startTime">زمان شروع بازه</param>
        /// <param name="endTime">زمان پایان بازه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد کاربران فعال</returns>
        Task<int> GetActiveUsersCountAsync(
            DateTime startTime,
            DateTime endTime,
            CancellationToken cancellationToken = default);

        Task AddAsync(LoginHistory loginHistory);
        Task<int> SaveChangesAsync();
    }

    public class LoginHistoryResult
    {
        public List<LoginHistory> Items { get; set; }
        public int TotalCount { get; set; }

        public IEnumerable<TResult> Select<TResult>(Func<LoginHistory, TResult> selector)
        {
            return Items.Select(selector);
        }
    }
} 