
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserTwoFactorDisabledEvent : DomainEventBase
    {
        public UserTwoFactorDisabledEvent(Guid userId) 
            : base(userId, "User")
        {
            MetaData["DisabledAt"] = DateTime.UtcNow;
        }
    }
}
