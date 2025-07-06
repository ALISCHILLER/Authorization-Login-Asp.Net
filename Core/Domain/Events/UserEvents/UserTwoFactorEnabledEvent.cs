
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserTwoFactorEnabledEvent : DomainEventBase
    {
        public UserTwoFactorEnabledEvent(Guid userId) 
            : base(userId, "User")
        {
            MetaData["EnabledAt"] = DateTime.UtcNow;
        }
    }
}
