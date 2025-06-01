using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس کش برای مدیریت عملیات کش‌گذاری
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// دریافت مقدار از کش
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="key">کلید</param>
        /// <returns>مقدار کش شده یا null</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// ذخیره مقدار در کش
        /// </summary>
        /// <typeparam name="T">نوع داده</typeparam>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        /// <param name="expiration">زمان انقضا (اختیاری)</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// حذف مقدار از کش
        /// </summary>
        /// <param name="key">کلید</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// بررسی وجود کلید در کش
        /// </summary>
        /// <param name="key">کلید</param>
        /// <returns>true اگر وجود داشته باشد</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// تمدید زمان انقضای کش
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="expiration">زمان انقضای جدید</param>
        Task ExtendAsync(string key, TimeSpan expiration);
    }
}