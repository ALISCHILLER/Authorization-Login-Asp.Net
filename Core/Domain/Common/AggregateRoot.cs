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

        // Overriding methods to ensure domain events are raised.
        // The actual state change is handled by the base.MarkAsUpdated/Deleted/Restored methods.

        public virtual void UpdateAuditable(string? modifiedByUserId = null)
        {
            base.MarkAsUpdated(modifiedByUserId);
            AddDomainEvent(new EntityUpdatedEvent(Id, GetType().Name, modifiedByUserId));
        }

        public virtual void DeleteAuditable(string? deletedByUserId = null)
        {
            base.MarkAsDeleted(deletedByUserId);
            // Note: EntityDeletedEvent might imply a hard delete.
            // If this is for soft delete, a different event like EntitySoftDeletedEvent might be more appropriate.
            AddDomainEvent(new EntityDeletedEvent(Id, GetType().Name, deletedByUserId));
        }

        public virtual void RestoreAuditable(string? restoredByUserId = null)
        {
            base.MarkAsRestored(restoredByUserId);
            AddDomainEvent(new EntityRestoredEvent(Id, GetType().Name, restoredByUserId));
        }
    }
}