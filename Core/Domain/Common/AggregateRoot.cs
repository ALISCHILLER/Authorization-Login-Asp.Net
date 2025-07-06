using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Events;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public abstract class AggregateRoot : BaseEntity, IAggregateRoot
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        protected AggregateRoot() : base()
        {
        }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));

            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public override void Update(Guid? modifiedBy = null)
        {
            base.Update(modifiedBy);
            AddDomainEvent(new EntityUpdatedEvent(Id, GetType().Name, modifiedBy));
        }

        public override void Delete(Guid? deletedBy = null)
        {
            base.Delete(deletedBy);
            AddDomainEvent(new EntityDeletedEvent(Id, GetType().Name, deletedBy));
        }

        public override void Restore(Guid? restoredBy = null)
        {
            base.Restore(restoredBy);
            AddDomainEvent(new EntityRestoredEvent(Id, GetType().Name, restoredBy));
        }
    }
}