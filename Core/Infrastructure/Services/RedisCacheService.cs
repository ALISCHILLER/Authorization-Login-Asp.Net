using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using StackExchange.Redis;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly TimeSpan _defaultExpiration;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            TimeSpan? defaultExpiration = null)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Cache key cannot be empty", nameof(key));

            try
            {
                var db = _redis.GetDatabase();
                var value = await db.StringGetAsync(key);

                if (!value.HasValue)
                {
                    return default!;
                }

                return JsonSerializer.Deserialize<T>(value!)!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis cache for key {Key}", key);
                return default!;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                var serializedValue = JsonSerializer.Serialize(value);
                await db.StringSetAsync(key, serializedValue, expiration ?? _defaultExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in Redis cache for key {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key} from Redis cache", key);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _redis.GetDatabase();
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of key {Key} in Redis cache", key);
                return false;
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
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

            return value!;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<T> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            value = factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value!;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, Func<Task<bool>> condition, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            if (await condition())
            {
                value = await factory();
                if (value != null)
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }
            }

            return value!;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<T> factory, Func<bool> condition, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            if (value != null)
            {
                return value;
            }

            if (condition())
            {
                value = factory();
                if (value != null)
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }
            }

            return value!;
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            // Note: Redis doesn't have a direct "clear all" operation
            // This would need to be implemented based on your specific requirements
            throw new NotImplementedException("Clear all operation is not supported in Redis cache service");
        }

        public async Task ExtendAsync(string key, TimeSpan extension, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _cache.GetStringAsync(key, cancellationToken);
                if (string.IsNullOrEmpty(value))
                    return;

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = extension
                };

                await _cache.SetStringAsync(key, value, options, cancellationToken);
                _logger.LogDebug("Cache key {Key} extended for {Duration}", key, extension);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending cache key {Key}", key);
                throw;
            }
        }
    }
}