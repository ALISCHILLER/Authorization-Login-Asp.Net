using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Cache
{
    /// <summary>
    /// پیاده‌سازی سرویس کش با استفاده از IDistributedCache
    /// </summary>
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DistributedCacheService(
            IDistributedCache cache,
            ILogger<DistributedCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        /// <inheritdoc/>
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrEmpty(value))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(value, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت مقدار از کش با کلید {Key}", key);
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }

                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, jsonValue, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ذخیره مقدار در کش با کلید {Key}", key);
            }
        }

        /// <inheritdoc/>
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف مقدار از کش با کلید {Key}", key);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی وجود کلید {Key} در کش", key);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await GetAsync<T>(key, cancellationToken);
                if (value != null)
                {
                    return value;
                }

                value = await factory();
                if (value != null)
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }

                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت یا ایجاد مقدار در کش با کلید {Key}", key);
                return await factory();
            }
        }

        /// <inheritdoc/>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // توجه: این متد در IDistributedCache وجود ندارد
                // در صورت نیاز به پاک کردن همه کش، باید از روش‌های دیگری استفاده کرد
                // مثلاً استفاده از Redis یا تغییر کلیدها
                _logger.LogWarning("پاک کردن همه کش در DistributedCache پشتیبانی نمی‌شود");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاک کردن کش");
            }
        }

        /// <inheritdoc/>
        public async Task ExtendAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = expiration
                    };
                    await _cache.SetStringAsync(key, value, options, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تمدید زمان انقضای کش با کلید {Key}", key);
            }
        }
    }
} 