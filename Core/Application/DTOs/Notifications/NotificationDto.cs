using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType NotificationType { get; set; }
        public NotificationPriority NotificationPriority { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsPersistent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}