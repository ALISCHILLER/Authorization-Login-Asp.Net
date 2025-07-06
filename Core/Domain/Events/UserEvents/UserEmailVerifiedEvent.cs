
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserEmailVerifiedEvent : DomainEventBase
    {
        public UserEmailVerifiedEvent(Guid userId) 
            : base(userId, "User")
        {
        }
    }
}
