using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// کلاس پایه برای رویدادهای دامنه
    /// این کلاس پیاده‌سازی پایه برای تمام رویدادهای دامنه را فراهم می‌کند
    /// </summary>
    public class DomainEvent : IDomainEvent
    {
        private readonly Dictionary<string, object> _metadata;

        /// <summary>
        /// شناسه رویداد
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// زمان رخداد
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// نوع رویداد
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// شناسه موجودیت مرتبط
        /// </summary>
        public Guid EntityId { get; }

        /// <summary>
        /// نوع موجودیت مرتبط
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public Guid? UserId { get; }

        /// <summary>
        /// نسخه رویداد
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// اطلاعات اضافی رویداد
        /// </summary>
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="entityId">شناسه موجودیت مرتبط</param>
        /// <param name="entityType">نوع موجودیت مرتبط</param>
        /// <param name="userId">شناسه کاربر ایجاد کننده</param>
        public DomainEvent(Guid entityId, string entityType, Guid? userId = null)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventType = GetType().Name;
            EntityId = entityId;
            EntityType = entityType;
            OccurredOn = DateTime.UtcNow;
            Version = 1;
            _metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// سازنده با اطلاعات موجودیت
        /// </summary>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="userId">شناسه کاربر</param>
        protected DomainEvent(Guid? entityId, string entityType, Guid? userId = null) : this()
        {
            EntityId = entityId;
            EntityType = entityType;
            UserId = userId;
        }

        /// <summary>
        /// افزودن متادیتا به رویداد
        /// </summary>
        /// <param name="key">کلید</param>
        /// <param name="value">مقدار</param>
        public void AddMetadata(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _metadata[key] = value;
        }

        /// <summary>
        /// دریافت متادیتا از رویداد
        /// </summary>
        /// <typeparam name="T">نوع مقدار</typeparam>
        /// <param name="key">کلید</param>
        /// <returns>مقدار</returns>
        public T GetMetadata<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!_metadata.ContainsKey(key))
                throw new KeyNotFoundException($"Metadata with key '{key}' not found.");

            return (T)_metadata[key];
        }

        /// <summary>
        /// بررسی وجود متادیتا
        /// </summary>
        /// <param name="key">کلید</param>
        /// <returns>نتیجه بررسی</returns>
        public bool HasMetadata(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            return _metadata.ContainsKey(key);
        }

        /// <summary>
        /// حذف متادیتا
        /// </summary>
        /// <param name="key">کلید</param>
        public void RemoveMetadata(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _metadata.Remove(key);
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"{EventType} - {Id} - {OccurredOn:yyyy-MM-dd HH:mm:ss} - Version {Version}";
        }
    }
}