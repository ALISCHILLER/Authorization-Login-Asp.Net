using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications
{
    public class UpdateNotificationRequest
    {
        public Guid Id { get; set; }
        public bool IsRead { get; set; }
    }
} 