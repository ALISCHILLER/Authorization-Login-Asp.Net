
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserUpdatedEvent : DomainEventBase
    {
        public UserUpdatedEvent(Guid userId) 
            : base(userId, "User")
        {
        }
    }
}
