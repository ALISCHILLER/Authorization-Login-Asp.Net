using System;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Events;

namespace Authorization_Login_Asp.Net.Core.Domain.Events.UserEvents
{
    /// <summary>
    /// رویداد ایجاد کاربر جدید
    /// </summary>
    public class UserCreatedEvent : DomainEvent
    {
        public User User { get; }
        public string Email { get; }
        public string Username { get; }

        public UserCreatedEvent(User user)
        {
            User = user;
            Email = user.Email;
            Username = user.Username;
        }
    }
}