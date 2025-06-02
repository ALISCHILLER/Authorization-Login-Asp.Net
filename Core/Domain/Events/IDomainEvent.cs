using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// رابط پایه برای رویدادهای دامنه
    /// این رابط برای تمام رویدادهای دامنه استفاده می‌شود و شامل اطلاعات پایه مورد نیاز است
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// شناسه یکتای رویداد
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// تاریخ وقوع رویداد
        /// </summary>
        DateTime OccurredOn { get; }

        /// <summary>
        /// نوع رویداد
        /// </summary>
        string EventType { get; }

        /// <summary>
        /// شناسه موجودیت مرتبط با رویداد (در صورت وجود)
        /// </summary>
        Guid? EntityId { get; }

        /// <summary>
        /// نام موجودیت مرتبط با رویداد (در صورت وجود)
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// اطلاعات اضافی رویداد
        /// </summary>
        object AdditionalData { get; }
    }
}