
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class EntityDeletedEvent : DomainEventBase
    {
        public EntityDeletedEvent(Guid entityId, string entityType, Guid? userId = null) 
            : base(entityId, entityType, userId)
        {
        }
    }
}
