using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Core.Application.Filters
{
    /// <summary>
    /// فیلتر برای کش کردن پاسخ‌ها
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CacheFilterAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _cacheKey;
        private readonly int _durationSeconds;
        private readonly bool _varyByUser;
        private readonly bool _varyByQuery;

        public CacheFilterAttribute(
            string cacheKey = null,
            int durationSeconds = 300,
            bool varyByUser = false,
            bool varyByQuery = false)
        {
            _cacheKey = cacheKey;
            _durationSeconds = durationSeconds;
            _varyByUser = varyByUser;
            _varyByQuery = varyByQuery;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<CacheFilterAttribute>)) as ILogger<CacheFilterAttribute>;
            var cache = context.HttpContext.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;

            if (cache == null)
            {
                logger?.LogError("IMemoryCache not found in DI container");
                await next();
                return;
            }

            var cacheKey = GenerateCacheKey(context);

            if (cache.TryGetValue(cacheKey, out var cachedResponse))
            {
                logger?.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                context.Result = new JsonResult(cachedResponse);
                return;
            }

            var executedContext = await next();

            if (executedContext.Result is ObjectResult objectResult)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(_durationSeconds))
                    .SetSlidingExpiration(TimeSpan.FromSeconds(_durationSeconds / 2))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        logger?.LogInformation("Cache entry {CacheKey} was evicted. Reason: {Reason}", key, reason);
                    });

                cache.Set(cacheKey, objectResult.Value, cacheEntryOptions);
                logger?.LogInformation("Cached response for key: {CacheKey}", cacheKey);
            }
        }

        private string GenerateCacheKey(ActionExecutingContext context)
        {
            var keyBuilder = new System.Text.StringBuilder();

            // کلید پایه
            keyBuilder.Append(_cacheKey ?? context.ActionDescriptor.DisplayName);

            // اضافه کردن شناسه کاربر در صورت نیاز
            if (_varyByUser)
            {
                var userId = context.HttpContext.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    keyBuilder.Append($"_user_{userId}");
                }
            }

            // اضافه کردن پارامترهای کوئری در صورت نیاز
            if (_varyByQuery)
            {
                var queryString = context.HttpContext.Request.QueryString.ToString();
                if (!string.IsNullOrEmpty(queryString))
                {
                    keyBuilder.Append($"_query_{queryString}");
                }
            }

            // اضافه کردن متد HTTP
            keyBuilder.Append($"_method_{context.HttpContext.Request.Method}");

            return keyBuilder.ToString();
        }
    }
} 