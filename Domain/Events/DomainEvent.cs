using System;

namespace Authorization_Login_Asp.Net.Domain.Events
{
    /// <summary>
    /// کلاس پایه برای رویدادهای دامنه
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime OccurredOn { get; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
    }
} 