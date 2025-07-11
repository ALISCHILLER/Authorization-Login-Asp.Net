using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications
{
    public class NotificationTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // Could be NotificationType enum if values align
        public string Priority { get; set; } // Could be NotificationPriority enum if values align
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
