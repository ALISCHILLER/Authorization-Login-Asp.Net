using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// رابط سرویس متریک‌ها برای جمع‌آوری و مدیریت آمار عملکرد برنامه
    /// </summary>
    public interface IMetricsService
    {
        /// <summary>
        /// افزایش تعداد درخواست‌های یک اندپوینت
        /// </summary>
        void IncrementRequestCount(string endpoint);

        /// <summary>
        /// افزایش تعداد خطاهای یک اندپوینت
        /// </summary>
        void IncrementErrorCount(string endpoint);

        /// <summary>
        /// ثبت زمان پاسخ یک اندپوینت
        /// </summary>
        void RecordResponseTime(string endpoint, long milliseconds);

        /// <summary>
        /// ثبت موفقیت‌آمیز بودن دسترسی به کش
        /// </summary>
        void RecordCacheHit(string cacheKey);

        /// <summary>
        /// ثبت عدم موفقیت در دسترسی به کش
        /// </summary>
        void RecordCacheMiss(string cacheKey);

        /// <summary>
        /// ثبت زمان عملیات پایگاه داده
        /// </summary>
        void RecordDatabaseOperation(string operation, long milliseconds);

        /// <summary>
        /// ثبت زمان فراخوانی سرویس‌های خارجی
        /// </summary>
        void RecordExternalServiceCall(string service, long milliseconds);

        /// <summary>
        /// دریافت تصویر لحظه‌ای از تمام متریک‌های جمع‌آوری شده
        /// </summary>
        MetricsSnapshot GetMetricsSnapshot();
    }

    /// <summary>
    /// پیاده‌سازی سرویس متریک‌ها با پشتیبانی از عملیات همزمان
    /// </summary>
    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        // دیکشنری‌های همزمان برای ذخیره متریک‌های مختلف
        private readonly ConcurrentDictionary<string, EndpointMetrics> _endpointMetrics = new();
        private readonly ConcurrentDictionary<string, long> _cacheMetrics = new();
        private readonly ConcurrentDictionary<string, long> _databaseMetrics = new();
        private readonly ConcurrentDictionary<string, long> _externalServiceMetrics = new();

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// افزایش تعداد درخواست‌های یک اندپوینت به صورت اتمیک
        /// </summary>
        public void IncrementRequestCount(string endpoint)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            metrics.IncrementRequests();
        }

        /// <summary>
        /// افزایش تعداد خطاهای یک اندپوینت به صورت اتمیک
        /// </summary>
        public void IncrementErrorCount(string endpoint)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            metrics.IncrementErrors();
        }

        /// <summary>
        /// ثبت زمان پاسخ یک اندپوینت با قفل‌گذاری مناسب
        /// </summary>
        public void RecordResponseTime(string endpoint, long milliseconds)
        {
            var metrics = _endpointMetrics.GetOrAdd(endpoint, _ => new EndpointMetrics());
            metrics.AddResponseTime(milliseconds);
        }

        /// <summary>
        /// ثبت موفقیت‌آمیز بودن دسترسی به کش با عملیات اتمیک
        /// </summary>
        public void RecordCacheHit(string cacheKey)
        {
            _cacheMetrics.AddOrUpdate("hits", 1, (_, count) => count + 1);
        }

        /// <summary>
        /// ثبت عدم موفقیت در دسترسی به کش با عملیات اتمیک
        /// </summary>
        public void RecordCacheMiss(string cacheKey)
        {
            _cacheMetrics.AddOrUpdate("misses", 1, (_, count) => count + 1);
        }

        /// <summary>
        /// ثبت زمان عملیات پایگاه داده با عملیات اتمیک
        /// </summary>
        public void RecordDatabaseOperation(string operation, long milliseconds)
        {
            _databaseMetrics.AddOrUpdate(operation, milliseconds, (_, oldValue) => oldValue + milliseconds);
        }

        /// <summary>
        /// ثبت زمان فراخوانی سرویس‌های خارجی با عملیات اتمیک
        /// </summary>
        public void RecordExternalServiceCall(string service, long milliseconds)
        {
            _externalServiceMetrics.AddOrUpdate(service, milliseconds, (_, oldValue) => oldValue + milliseconds);
        }

        /// <summary>
        /// ایجاد یک تصویر لحظه‌ای از تمام متریک‌ها با کپی عمیق برای جلوگیری از تغییرات همزمان
        /// </summary>
        public MetricsSnapshot GetMetricsSnapshot()
        {
            return new MetricsSnapshot
            {
                EndpointMetrics = _endpointMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()),
                CacheMetrics = _cacheMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                DatabaseMetrics = _databaseMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ExternalServiceMetrics = _externalServiceMetrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// کلاس نگهداری متریک‌های یک اندپوینت با پشتیبانی از عملیات همزمان
    /// </summary>
    public class EndpointMetrics
    {
        // فیلدهای خصوصی برای نگهداری متریک‌ها
        private long _totalRequests;      // تعداد کل درخواست‌ها
        private long _errorCount;         // تعداد کل خطاها
        private long _totalResponseTime;  // مجموع زمان‌های پاسخ
        private readonly object _syncLock = new();  // قفل برای عملیات همزمان

        // خواص فقط خواندنی برای دسترسی به متریک‌ها
        public long TotalRequests => _totalRequests;
        public long ErrorCount => _errorCount;
        public long TotalResponseTime => _totalResponseTime;
        public double AverageResponseTime => _totalRequests > 0 ? (double)_totalResponseTime / _totalRequests : 0;

        /// <summary>
        /// افزایش تعداد درخواست‌ها به صورت اتمیک
        /// </summary>
        public void IncrementRequests()
        {
            Interlocked.Increment(ref _totalRequests);
        }

        /// <summary>
        /// افزایش تعداد خطاها به صورت اتمیک
        /// </summary>
        public void IncrementErrors()
        {
            Interlocked.Increment(ref _errorCount);
        }

        /// <summary>
        /// افزودن زمان پاسخ با قفل‌گذاری مناسب
        /// </summary>
        public void AddResponseTime(long milliseconds)
        {
            lock (_syncLock)
            {
                _totalResponseTime += milliseconds;
            }
        }

        /// <summary>
        /// ایجاد یک کپی از متریک‌های اندپوینت
        /// </summary>
        public EndpointMetrics Clone()
        {
            return new EndpointMetrics
            {
                _totalRequests = _totalRequests,
                _errorCount = _errorCount,
                _totalResponseTime = _totalResponseTime
            };
        }
    }

    /// <summary>
    /// کلاس نگهداری تصویر لحظه‌ای از تمام متریک‌های سیستم
    /// </summary>
    public class MetricsSnapshot
    {
        /// <summary>
        /// متریک‌های اندپوینت‌ها
        /// </summary>
        public Dictionary<string, EndpointMetrics> EndpointMetrics { get; set; }

        /// <summary>
        /// متریک‌های کش (تعداد موفقیت‌ها و شکست‌ها)
        /// </summary>
        public Dictionary<string, long> CacheMetrics { get; set; }

        /// <summary>
        /// متریک‌های عملیات پایگاه داده
        /// </summary>
        public Dictionary<string, long> DatabaseMetrics { get; set; }

        /// <summary>
        /// متریک‌های فراخوانی سرویس‌های خارجی
        /// </summary>
        public Dictionary<string, long> ExternalServiceMetrics { get; set; }

        /// <summary>
        /// زمان ثبت تصویر لحظه‌ای
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}