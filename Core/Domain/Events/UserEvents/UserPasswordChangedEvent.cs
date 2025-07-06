
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserPasswordChangedEvent : DomainEventBase
    {
        public UserPasswordChangedEvent(Guid userId) 
            : base(userId, "User")
        {
            MetaData["ChangedAt"] = DateTime.UtcNow;
        }
    }
}
