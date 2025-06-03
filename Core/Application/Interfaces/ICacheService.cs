using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// رابط سرویس کش برای ذخیره و بازیابی داده‌ها
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// دریافت مقدار از کش
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="key">کلید</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>مقدار ذخیره شده در کش یا null اگر وجود نداشته باشد</returns>
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// ذخیره مقدار در کش
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        /// <param name="expiration">زمان انقضا</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف مقدار از کش
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود کلید در کش
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کلید در کش وجود داشته باشد</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت یا ایجاد مقدار در کش
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="key">کلید</param>
        /// <param name="factory">تابع ایجاد مقدار در صورت عدم وجود در کش</param>
        /// <param name="expiration">زمان انقضا</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>مقدار موجود در کش یا مقدار جدید ایجاد شده</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// پاک کردن همه مقادیر کش
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// تمدید زمان انقضای یک کلید
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="expiration">زمان انقضای جدید</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        Task ExtendAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);
    }
}