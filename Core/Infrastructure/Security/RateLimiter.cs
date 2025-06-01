using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    /// <summary>
    /// کلاس محدودکننده نرخ درخواست برای جلوگیری از حملات Brute Force
    /// </summary>
    public class RateLimiter
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimiter> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;
        private readonly ConcurrentDictionary<string, DateTime> _blacklist;
        private readonly int _blacklistDurationMinutes;
        private readonly int _maxFailedAttempts;

        public RateLimiter(
            IMemoryCache cache,
            ILogger<RateLimiter> logger,
            int blacklistDurationMinutes = 60,
            int maxFailedAttempts = 5)
        {
            _cache = cache;
            _logger = logger;
            _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
            _blacklist = new ConcurrentDictionary<string, DateTime>();
            _blacklistDurationMinutes = blacklistDurationMinutes;
            _maxFailedAttempts = maxFailedAttempts;
        }

        /// <summary>
        /// بررسی و افزایش تعداد تلاش‌های ورود
        /// </summary>
        /// <param name="key">کلید (معمولاً IP یا نام کاربری)</param>
        /// <param name="maxAttempts">حداکثر تعداد تلاش مجاز</param>
        /// <param name="windowMinutes">بازه زمانی به دقیقه</param>
        /// <returns>true اگر تعداد تلاش‌ها از حد مجاز کمتر باشد</returns>
        public async Task<bool> CheckAndIncrementAsync(string key, int maxAttempts, int windowMinutes)
        {
            try
            {
                // بررسی blacklist
                if (IsBlacklisted(key))
                {
                    _logger.LogWarning("Blocked request from blacklisted key: {Key}", key);
                    return false;
                }

                var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

                try
                {
                    await semaphore.WaitAsync();

                    var cacheKey = $"rate_limit_{key}";
                    var attempts = await _cache.GetOrCreateAsync(cacheKey, entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(windowMinutes);
                        return Task.FromResult(0);
                    });

                    if (attempts >= maxAttempts)
                    {
                        _logger.LogWarning("Rate limit exceeded for key: {Key}", key);
                        return false;
                    }

                    attempts++;
                    _cache.Set(cacheKey, attempts, TimeSpan.FromMinutes(windowMinutes));

                    // بررسی تعداد تلاش‌های ناموفق
                    if (attempts >= _maxFailedAttempts)
                    {
                        AddToBlacklist(key);
                        _logger.LogWarning("Added key to blacklist: {Key}", key);
                    }

                    return true;
                }
                finally
                {
                    semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting for key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// پاک کردن تعداد تلاش‌های یک کلید
        /// </summary>
        /// <param name="key">کلید</param>
        public void ResetAttempts(string key)
        {
            try
            {
                var cacheKey = $"rate_limit_{key}";
                _cache.Remove(cacheKey);
                _logger.LogInformation("Reset attempts for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting attempts for key: {Key}", key);
            }
        }

        /// <summary>
        /// دریافت تعداد تلاش‌های باقیمانده
        /// </summary>
        /// <param name="key">کلید</param>
        /// <returns>تعداد تلاش‌های باقیمانده</returns>
        public int GetRemainingAttempts(string key)
        {
            try
            {
                var cacheKey = $"rate_limit_{key}";
                return _cache.TryGetValue<int>(cacheKey, out var attempts) ? attempts : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining attempts for key: {Key}", key);
                return 0;
            }
        }

        /// <summary>
        /// اضافه کردن IP به لیست سیاه
        /// </summary>
        /// <param name="key">کلید</param>
        public void AddToBlacklist(string key)
        {
            _blacklist.TryAdd(key, DateTime.UtcNow.AddMinutes(_blacklistDurationMinutes));
        }

        /// <summary>
        /// حذف IP از لیست سیاه
        /// </summary>
        /// <param name="key">کلید</param>
        public void RemoveFromBlacklist(string key)
        {
            _blacklist.TryRemove(key, out _);
        }

        /// <summary>
        /// بررسی وجود IP در لیست سیاه
        /// </summary>
        /// <param name="key">کلید</param>
        /// <returns>true اگر IP در لیست سیاه باشد</returns>
        public bool IsBlacklisted(string key)
        {
            if (_blacklist.TryGetValue(key, out var expirationTime))
            {
                if (DateTime.UtcNow > expirationTime)
                {
                    RemoveFromBlacklist(key);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// دریافت لیست IP های موجود در لیست سیاه
        /// </summary>
        /// <returns>لیست IP های موجود در لیست سیاه</returns>
        public IEnumerable<string> GetBlacklistedKeys()
        {
            return _blacklist.Keys;
        }

        /// <summary>
        /// پاک کردن IP های منقضی شده از لیست سیاه
        /// </summary>
        public void CleanupBlacklist()
        {
            var expiredKeys = _blacklist
                .Where(x => DateTime.UtcNow > x.Value)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                RemoveFromBlacklist(key);
            }
        }
    }
}