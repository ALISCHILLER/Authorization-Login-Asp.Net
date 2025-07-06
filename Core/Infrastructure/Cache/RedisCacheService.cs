using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly DistributedCacheEntryOptions _defaultOptions;

        public RedisCacheService(
            IDistributedCache cache,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _defaultOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrEmpty(value))
                    return default!;

                return JsonSerializer.Deserialize<T>(value)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key {Key}", key);
                return default!;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = expiration.HasValue
                    ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration }
                    : _defaultOptions;

                var serializedValue = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence in cache for key {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null && !value.Equals(default(T)))
                return value;
            value = await factory();
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
            return value;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            // Not supported in IDistributedCache, so just a stub for now
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            // Not supported in IDistributedCache, so just a stub for now
            return Task.CompletedTask;
        }

        public Task ExtendAsync(string key, TimeSpan time, CancellationToken cancellationToken = default)
        {
            // Not supported in IDistributedCache, so just a stub for now
            return Task.CompletedTask;
        }
    }
}