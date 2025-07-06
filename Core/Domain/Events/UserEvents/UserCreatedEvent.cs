
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserCreatedEvent : DomainEventBase
    {
        public UserCreatedEvent(Guid userId) 
            : base(userId, "User")
        {
        }
    }
}
