using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Authorization_Login_Asp.Net.Infrastructure.Middleware
{
    public class CacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CacheMiddleware> _logger;
        private readonly IDistributedCache _cache;
        private readonly MiddlewareConfiguration _config;

        public CacheMiddleware(
            RequestDelegate next,
            ILogger<CacheMiddleware> logger,
            IDistributedCache cache,
            MiddlewareConfiguration config)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_config.EnableResponseCompression || !ShouldCache(context))
            {
                await _next(context);
                return;
            }

            var cacheKey = GenerateCacheKey(context);
            var cachedResponse = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                _logger.LogInformation("کش یافت شد برای کلید: {CacheKey}", cacheKey);
                await ReturnCachedResponse(context, cachedResponse);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                if (context.Response.StatusCode == 200)
                {
                    var response = await FormatResponse(context.Response);
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                        SlidingExpiration = TimeSpan.FromMinutes(2)
                    };

                    await _cache.SetStringAsync(cacheKey, response, cacheOptions);
                    _logger.LogInformation("پاسخ در کش ذخیره شد با کلید: {CacheKey}", cacheKey);
                }

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در مدیریت کش");
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private bool ShouldCache(HttpContext context)
        {
            // فقط درخواست‌های GET را کش می‌کنیم
            if (context.Request.Method != "GET")
                return false;

            // بررسی نوع محتوا
            var contentType = context.Response.ContentType?.ToLower();
            if (string.IsNullOrEmpty(contentType))
                return false;

            // انواع محتوایی که باید کش شوند
            var cacheableContentTypes = new[]
            {
                "application/json",
                "text/html",
                "text/plain",
                "application/xml"
            };

            return cacheableContentTypes.Any(type => contentType.StartsWith(type));
        }

        private string GenerateCacheKey(HttpContext context)
        {
            var keyBuilder = new System.Text.StringBuilder();
            keyBuilder.Append(context.Request.Path);
            keyBuilder.Append(context.Request.QueryString);

            // اضافه کردن هدرهای مهم به کلید کش
            var importantHeaders = new[] { "Accept", "Accept-Language", "Authorization" };
            foreach (var header in importantHeaders)
            {
                if (context.Request.Headers.TryGetValue(header, out var value))
                {
                    keyBuilder.Append($":{header}={value}");
                }
            }

            return keyBuilder.ToString();
        }

        private async Task ReturnCachedResponse(HttpContext context, string cachedResponse)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(cachedResponse);
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
} 