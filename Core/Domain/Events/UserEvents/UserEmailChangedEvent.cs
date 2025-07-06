
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Events
{
    public class UserEmailChangedEvent : DomainEventBase
    {
        public string NewEmail { get; }

        public UserEmailChangedEvent(Guid userId, string newEmail) 
            : base(userId, "User")
        {
            NewEmail = newEmail;
            MetaData["NewEmail"] = newEmail;
        }
    }
}
