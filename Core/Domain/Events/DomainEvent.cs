using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// کلاس پایه برای رویدادهای دامنه
    /// این کلاس پیاده‌سازی پایه برای تمام رویدادهای دامنه را فراهم می‌کند
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// شناسه یکتای رویداد
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// زمان وقوع رویداد
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// نوع رویداد
        /// </summary>
        public string EventType => GetType().Name;

        /// <summary>
        /// شناسه موجودیت مرتبط با رویداد
        /// </summary>
        public Guid? EntityId { get; protected set; }

        /// <summary>
        /// نام موجودیت مرتبط با رویداد
        /// </summary>
        public string EntityType { get; protected set; }

        /// <summary>
        /// اطلاعات اضافی رویداد
        /// </summary>
        public object AdditionalData { get; protected set; }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        /// <summary>
        /// سازنده با تاریخ مشخص
        /// </summary>
        /// <param name="occurredOn">تاریخ وقوع رویداد</param>
        protected DomainEvent(DateTime occurredOn)
        {
            Id = Guid.NewGuid();
            OccurredOn = occurredOn;
        }

        /// <summary>
        /// سازنده با اطلاعات موجودیت
        /// </summary>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="additionalData">اطلاعات اضافی</param>
        protected DomainEvent(Guid entityId, string entityType, object additionalData = null)
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EntityId = entityId;
            EntityType = entityType;
            AdditionalData = additionalData;
        }

        /// <summary>
        /// تبدیل رویداد به رشته
        /// </summary>
        public override string ToString()
        {
            return $"{EventType} - {Id} - {OccurredOn:yyyy-MM-dd HH:mm:ss}";
        }
    }
}