using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    /// <summary>
    /// رابط پایه برای رویدادهای دامنه
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
    }
}