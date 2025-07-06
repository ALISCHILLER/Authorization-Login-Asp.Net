
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class EntityRestoredEvent : DomainEventBase
    {
        public EntityRestoredEvent(Guid entityId, string entityType, Guid? userId = null) 
            : base(entityId, entityType, userId)
        {
        }
    }
}
