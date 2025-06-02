using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Events;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای موجودیت‌های تجمعی
    /// </summary>
    public abstract class AggregateRoot : BaseEntity
    {
        private readonly List<DomainEvent> _domainEvents = new();

        /// <summary>
        /// رویدادهای دامنه
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// افزودن رویداد دامنه
        /// </summary>
        /// <param name="domainEvent">رویداد دامنه</param>
        protected void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// پاک کردن رویدادهای دامنه
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// اعمال تغییرات
        /// </summary>
        public override void Update()
        {
            base.Update();
            AddDomainEvent(new DomainEvent());
        }

        /// <summary>
        /// حذف منطقی
        /// </summary>
        public override void Delete()
        {
            base.Delete();
            AddDomainEvent(new DomainEvent());
        }

        /// <summary>
        /// بازیابی
        /// </summary>
        public override void Restore()
        {
            base.Restore();
            AddDomainEvent(new DomainEvent());
        }
    }
} 