using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// رابط رویداد دامنه
    /// این اینترفیس اطلاعات پایه رویدادها را تعریف می‌کند
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// شناسه رویداد
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// زمان رخداد
        /// </summary>
        DateTime OccurredOn { get; }

        /// <summary>
        /// نوع رویداد
        /// </summary>
        string EventType { get; }

        /// <summary>
        /// شناسه موجودیت مرتبط
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        /// نوع موجودیت مرتبط
        /// </summary>
        string EntityType { get; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        Guid? UserId { get; }

        /// <summary>
        /// نسخه رویداد
        /// </summary>
        int Version { get; }

        /// <summary>
        /// اطلاعات اضافی رویداد
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }
    }
}