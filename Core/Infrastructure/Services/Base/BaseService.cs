using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base
{
    /// <summary>
    /// کلاس پایه برای تمام سرویس‌ها
    /// </summary>
    public abstract class BaseService
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly ILogger _logger;
        protected readonly IMemoryCache _cache;
        protected readonly MemoryCacheEntryOptions _cacheOptions;

        protected BaseService(
            IUnitOfWork unitOfWork,
            ILogger logger,
            IMemoryCache cache,
            int cacheExpirationMinutes = 30)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes / 2));
        }

        /// <summary>
        /// اجرای عملیات در یک تراکنش
        /// </summary>
        protected async Task<T> ExecuteInTransaction<T>(Func<Task<T>> operation, string operationName)
        {
            try
            {
                _logger.LogInformation($"شروع عملیات {operationName}");
                var result = await operation();
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"عملیات {operationName} با موفقیت انجام شد");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در عملیات {operationName}");
                throw;
            }
        }

        /// <summary>
        /// اجرای عملیات در یک تراکنش بدون مقدار بازگشتی
        /// </summary>
        protected async Task ExecuteInTransaction(Func<Task> operation, string operationName)
        {
            try
            {
                _logger.LogInformation($"شروع عملیات {operationName}");
                await operation();
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"عملیات {operationName} با موفقیت انجام شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در عملیات {operationName}");
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود داده
        /// </summary>
        protected async Task<bool> ExistsAsync<T>(Func<Task<bool>> check, string entityName)
        {
            try
            {
                return await check();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در بررسی وجود {entityName}");
                throw;
            }
        }

        /// <summary>
        /// اعتبارسنجی داده
        /// </summary>
        protected void Validate<T>(T entity, string entityName) where T : class
        {
            if (entity == null)
            {
                var error = $"{entityName} نمی‌تواند خالی باشد";
                _logger.LogError(error);
                throw new ArgumentNullException(nameof(entity), error);
            }
        }

        /// <summary>
        /// اجرای عملیات با مدیریت خطا و لاگینگ
        /// </summary>
        protected async Task<T> ExecuteWithLoggingAsync<T>(string operationName, Func<Task<T>> operation)
        {
            try
            {
                _logger.LogInformation("شروع عملیات {OperationName}", operationName);
                var result = await operation();
                _logger.LogInformation("عملیات {OperationName} با موفقیت انجام شد", operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در عملیات {OperationName}", operationName);
                throw new DomainException($"خطا در عملیات {operationName}", ex);
            }
        }

        /// <summary>
        /// اجرای عملیات با مدیریت خطا و لاگینگ (بدون مقدار برگشتی)
        /// </summary>
        protected async Task ExecuteWithLoggingAsync(string operationName, Func<Task> operation)
        {
            try
            {
                _logger.LogInformation("شروع عملیات {OperationName}", operationName);
                await operation();
                _logger.LogInformation("عملیات {OperationName} با موفقیت انجام شد", operationName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در عملیات {OperationName}", operationName);
                throw new DomainException($"خطا در عملیات {operationName}", ex);
            }
        }

        /// <summary>
        /// دریافت مقدار از کش یا اجرای عملیات و ذخیره در کش
        /// </summary>
        protected async Task<T> GetOrSetCacheAsync<T>(string cacheKey, Func<Task<T>> getData)
        {
            if (_cache.TryGetValue(cacheKey, out T cachedValue))
            {
                return cachedValue;
            }

            var value = await getData();
            _cache.Set(cacheKey, value, _cacheOptions);
            return value;
        }

        /// <summary>
        /// پاک کردن مقدار از کش
        /// </summary>
        protected void RemoveFromCache(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// پاک کردن چند مقدار از کش با استفاده از الگو
        /// </summary>
        protected void RemoveFromCacheByPattern(string pattern)
        {
            var cacheEntries = _cache.GetType().GetProperty("EntriesCollection")?.GetValue(_cache);
            if (cacheEntries == null) return;

            foreach (var entry in cacheEntries as dynamic)
            {
                var key = entry.GetType().GetProperty("Key")?.GetValue(entry)?.ToString();
                if (key != null && key.Contains(pattern))
                {
                    _cache.Remove(key);
                }
            }
        }
    }
} 