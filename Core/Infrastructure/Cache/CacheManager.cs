using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Cache
{
    /// <summary>
    /// کلاس مدیریت کش
    /// </summary>
    public class CacheManager : ICacheManager
    {
        private readonly ICacheService _cacheService;
        private readonly ILogger<CacheManager> _logger;
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(30);

        public CacheManager(
            ICacheService cacheService,
            ILogger<CacheManager> logger)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت یا ایجاد مقدار در کش
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cacheService.GetOrCreateAsync(
                    key,
                    factory,
                    duration ?? _defaultCacheDuration,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت یا ایجاد مقدار در کش با کلید {Key}", key);
                return await factory();
            }
        }

        /// <summary>
        /// حذف مقدار از کش
        /// </summary>
        public async Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.RemoveAsync(key, cancellationToken);
                _logger.LogInformation("مقدار با کلید {Key} از کش حذف شد", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف مقدار از کش با کلید {Key}", key);
            }
        }

        /// <summary>
        /// حذف چند مقدار از کش
        /// </summary>
        public async Task RemoveAsync(
            IEnumerable<string> keys,
            CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key, cancellationToken);
            }
        }

        /// <summary>
        /// حذف همه مقادیر با پیشوند مشخص
        /// </summary>
        public async Task RemoveByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.RemoveByPrefixAsync(prefix, cancellationToken);
                _logger.LogInformation("همه مقادیر با پیشوند {Prefix} از کش حذف شدند", prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف مقادیر با پیشوند {Prefix} از کش", prefix);
            }
        }

        /// <summary>
        /// بررسی وجود مقدار در کش
        /// </summary>
        public async Task<bool> ExistsAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cacheService.ExistsAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وجود مقدار در کش با کلید {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// دریافت مقدار از کش
        /// </summary>
        public async Task<T> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _cacheService.GetAsync<T>(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت مقدار از کش با کلید {Key}", key);
                return default;
            }
        }

        /// <summary>
        /// ذخیره مقدار در کش
        /// </summary>
        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? duration = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.SetAsync(
                    key,
                    value,
                    duration ?? _defaultCacheDuration,
                    cancellationToken);
                _logger.LogInformation("مقدار با کلید {Key} در کش ذخیره شد", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ذخیره مقدار در کش با کلید {Key}", key);
            }
        }

        /// <summary>
        /// تمدید مدت زمان کش
        /// </summary>
        public async Task ExtendAsync(
            string key,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.ExtendAsync(key, duration, cancellationToken);
                _logger.LogInformation("مدت زمان کش با کلید {Key} تمدید شد", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تمدید مدت زمان کش با کلید {Key}", key);
            }
        }

        /// <summary>
        /// پاک کردن همه کش
        /// </summary>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _cacheService.ClearAsync(cancellationToken);
                _logger.LogInformation("همه کش پاک شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاک کردن همه کش");
            }
        }
    }
} 