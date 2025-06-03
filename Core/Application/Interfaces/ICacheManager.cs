using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// رابط مدیریت کش
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// دریافت یا ایجاد مقدار در کش
        /// </summary>
        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف مقدار از کش
        /// </summary>
        Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف چند مقدار از کش
        /// </summary>
        Task RemoveAsync(
            IEnumerable<string> keys,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف همه مقادیر با پیشوند مشخص
        /// </summary>
        Task RemoveByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود مقدار در کش
        /// </summary>
        Task<bool> ExistsAsync(
            string key,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت مقدار از کش
        /// </summary>
        Task<T> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ذخیره مقدار در کش
        /// </summary>
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// تمدید مدت زمان کش
        /// </summary>
        Task ExtendAsync(
            string key,
            TimeSpan duration,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// پاک کردن همه کش
        /// </summary>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
} 