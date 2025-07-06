
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class EntityUpdatedEvent : DomainEventBase
    {
        public EntityUpdatedEvent(Guid entityId, string entityType, Guid? userId = null) 
            : base(entityId, entityType, userId)
        {
        }
    }
}
